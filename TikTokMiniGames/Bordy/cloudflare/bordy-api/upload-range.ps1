<#
  upload-range.ps1 - batch-upload already-generated daily JSON files to KV, then verify each.

  Uploads the EXISTING <date>.json files (does NOT regenerate), so what goes live is exactly
  what was validated. Run from cloudflare/bordy-api.

  Usage:
    .\upload-range.ps1                         # today (UTC) + next 13 days = rolling 14-day window
    .\upload-range.ps1 -Dates 20260817,20260818   # only specific days

  If PowerShell blocks it, first run:  Set-ExecutionPolicy -Scope Process -Bypass
#>
param(
    [string[]]$Dates = $null
)

# Default: today (UTC) + the next 13 days = a rolling 14-day window.
if (-not $Dates) {
    $Dates = 0..13 | ForEach-Object { [DateTime]::UtcNow.Date.AddDays($_).ToString("yyyyMMdd") }
}

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot
$WorkerUrl = "https://bordy-api.brainless.workers.dev"

$fail = 0
foreach ($d in $Dates) {
    $file = "$d.json"
    if (-not (Test-Path $file)) {
        Write-Warning "skip $d - $file not found"
        $fail++
        continue
    }

    Write-Host "==> Uploading daily:$d ..."
    npx wrangler kv key put --binding=BORDY_KV "daily:$d" --path="$file"
    if ($LASTEXITCODE -ne 0) { Write-Warning "wrangler put failed for $d"; $fail++; continue }

    try {
        $r = Invoke-RestMethod -Uri "$WorkerUrl/api/daily/$d.json" -Method GET
        $clues = ($r.givens | Where-Object { $_ -eq $true }).Count
        Write-Host "    OK  date=$($r.date)  size=$($r.size)  givens=$clues/36  edges=$($r.edges.Count)"
    } catch {
        Write-Warning "verify failed for $d (KV may need a few seconds)"
        $fail++
    }
}

if ($fail -eq 0) { Write-Host "`nAll daily puzzles are live." }
else { Write-Warning "`nDone with $fail problem(s) - re-run to retry those dates." }
