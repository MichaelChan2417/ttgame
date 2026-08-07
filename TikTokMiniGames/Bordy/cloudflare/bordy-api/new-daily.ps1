<#
  new-daily.ps1 — one click per day: generate a fresh unique puzzle → upload to KV → verify.

  用法（在 cloudflare/bordy-api 目录下）:
    .\new-daily.ps1                 # 今天 (UTC)
    .\new-daily.ps1 -Date 20260805  # 指定某天（可提前把未来几天都灌好）

  首次运行若被 PowerShell 拦：先执行  Set-ExecutionPolicy -Scope Process -Bypass
#>
param(
    [string]$Date = ([DateTime]::UtcNow.ToString("yyyyMMdd"))
)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot
$WorkerUrl = "https://bordy-api.brainless.workers.dev"

Write-Host "==> [1/3] Generating puzzle for $Date ..."
node generate-daily.js $Date
if ($LASTEXITCODE -ne 0) { Write-Error "generate-daily.js failed"; exit 1 }

$file = "$Date.json"
if (-not (Test-Path $file)) { Write-Error "expected $file was not created"; exit 1 }

Write-Host "==> [2/3] Uploading to KV: daily:$Date ..."
npx wrangler kv key put --binding=BORDY_KV "daily:$Date" --path="$file" --remote
if ($LASTEXITCODE -ne 0) { Write-Error "wrangler kv put failed"; exit 1 }

Write-Host "==> [3/3] Verifying $WorkerUrl/api/daily/$Date.json ..."
try {
    $r = Invoke-RestMethod -Uri "$WorkerUrl/api/daily/$Date.json" -Method GET
    $clues = ($r.givens | Where-Object { $_ -eq $true }).Count
    Write-Host "OK ✅  date=$($r.date)  size=$($r.size)  givens=$clues/36  edges=$($r.edges.Count)"
    Write-Host "Daily $Date is live."
} catch {
    Write-Error "verify failed (KV may need a few seconds): $_"
    exit 1
}
