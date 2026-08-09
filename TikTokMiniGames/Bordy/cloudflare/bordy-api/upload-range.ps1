<#
  upload-range.ps1 — batch-upload already-generated daily JSON files to KV, then verify each.

  Uploads the EXISTING <date>.json files (does NOT regenerate), so what goes live is exactly
  what was validated. Run from cloudflare/bordy-api.

  用法（在 cloudflare/bordy-api 目录下）:
    .\upload-range.ps1                       # 上传下面默认的一批日期
    .\upload-range.ps1 -Dates 20260810,20260811   # 只传指定几天

  首次运行若被 PowerShell 拦：先执行  Set-ExecutionPolicy -Scope Process -Bypass
#>
param(
    [string[]]$Dates = @(
        "20260809","20260810","20260811","20260812","20260813","20260814","20260815","20260816"
    )
)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot
$WorkerUrl = "https://bordy-api.brainless.workers.dev"

$fail = 0
foreach ($d in $Dates) {
    $file = "$d.json"
    if (-not (Test-Path $file)) {
        Write-Warning "skip $d — $file not found"
        $fail++
        continue
    }

    Write-Host "==> Uploading daily:$d ..."
    npx wrangler kv key put --binding=BORDY_KV "daily:$d" --path="$file" --remote
    if ($LASTEXITCODE -ne 0) { Write-Warning "wrangler put failed for $d"; $fail++; continue }

    try {
        $r = Invoke-RestMethod -Uri "$WorkerUrl/api/daily/$d.json" -Method GET
        $clues = ($r.givens | Where-Object { $_ -eq $true }).Count
        Write-Host "    OK  date=$($r.date)  size=$($r.size)  givens=$clues/36  edges=$($r.edges.Count)"
    } catch {
        Write-Warning "verify failed for $d (KV may need a few seconds): $_"
        $fail++
    }
}

if ($fail -eq 0) { Write-Host "`nAll daily puzzles are live." }
else { Write-Warning "`nDone with $fail problem(s) — re-run to retry those dates." }
