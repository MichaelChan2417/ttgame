<#
  upload-daily.ps1 - upload one day's daily puzzle JSON to Cloudflare KV and verify.

  Usage (from cloudflare/bordy-api):
    .\upload-daily.ps1                      # today (UTC), reads <date>.json
    .\upload-daily.ps1 -Date 20260803       # specific day, reads 20260803.json
    .\upload-daily.ps1 -Date 20260803 -File .\mypuzzle.json
    .\upload-daily.ps1 -Deploy              # also deploy the Worker first (only if Worker code changed)

  Tip: to upload many days at once, use .\upload-range.ps1 instead.
  If PowerShell blocks it, first run:  Set-ExecutionPolicy -Scope Process -Bypass
#>
param(
    [string]$Date = ([DateTime]::UtcNow.ToString("yyyyMMdd")),
    [string]$File = "",
    [switch]$Deploy
)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

$WorkerUrl = "https://bordy-api.brainless.workers.dev"

if ([string]::IsNullOrWhiteSpace($File)) { $File = Join-Path $PSScriptRoot "$Date.json" }

# 1) file exists + valid JSON
if (-not (Test-Path $File)) {
    Write-Error "puzzle file not found: $File  (generate it first, e.g. node generate-daily.js $Date)"
    exit 1
}
Write-Host "==> Validating JSON: $File"
try {
    $dto = Get-Content $File -Raw -Encoding UTF8 | ConvertFrom-Json
    if (-not $dto.solution -or -not $dto.givens) { throw "missing solution / givens field" }
} catch {
    Write-Error "invalid JSON: $_"
    exit 1
}

# 2) optional: deploy Worker (only when Worker code changed)
if ($Deploy) {
    Write-Host "==> Deploying Worker ..."
    npx wrangler deploy
}

# 3) upload to KV under key daily:<date>
Write-Host "==> Uploading to KV: daily:$Date  (from $File)"
npx wrangler kv key put --binding=BORDY_KV "daily:$Date" --path="$File" --remote
if ($LASTEXITCODE -ne 0) { Write-Error "wrangler kv put failed"; exit 1 }

# 4) verify it is live
$url = "$WorkerUrl/api/daily/$Date.json"
Write-Host "==> Verifying $url"
try {
    $resp = Invoke-RestMethod -Uri $url -Method GET
    $edgeCount = if ($resp.edges) { $resp.edges.Count } else { 0 }
    Write-Host "OK  date=$($resp.date) size=$($resp.size) edges=$edgeCount"
    Write-Host "Daily is live: daily:$Date"
} catch {
    Write-Error "verify failed (KV may need a few seconds)"
    exit 1
}
