<#
  new-range.ps1 — generate a stretch of daily puzzles (one JSON per UTC day).
  Does NOT upload unless you pass -Upload.

  用法（在 cloudflare/bordy-api 目录下）:
    .\new-range.ps1                         # 从今天(UTC)起 30 天，只写本地 JSON
    .\new-range.ps1 -Days 14                # 从今天起 14 天
    .\new-range.ps1 -From 20260816 -Days 7  # 指定起始日
    .\new-range.ps1 -From 20260816 -To 20260915
    .\new-range.ps1 -Days 30 -Upload        # 生成后写入生产 KV（需 wrangler 已登录）

  首次运行若被 PowerShell 拦：先执行  Set-ExecutionPolicy -Scope Process -Bypass
#>
param(
    [string]$From = ([DateTime]::UtcNow.ToString("yyyyMMdd")),
    [string]$To = "",
    [int]$Days = 30,
    [switch]$Upload
)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot
$WorkerUrl = "https://bordy-api.brainless.workers.dev"

if ($From -notmatch '^\d{8}$') { Write-Error "Bad -From (expected YYYYMMDD)"; exit 1 }

$start = [DateTime]::ParseExact($From, "yyyyMMdd", [Globalization.CultureInfo]::InvariantCulture)
if ($To) {
    if ($To -notmatch '^\d{8}$') { Write-Error "Bad -To (expected YYYYMMDD)"; exit 1 }
    $end = [DateTime]::ParseExact($To, "yyyyMMdd", [Globalization.CultureInfo]::InvariantCulture)
} else {
    if ($Days -lt 1) { Write-Error "-Days must be >= 1"; exit 1 }
    $end = $start.AddDays($Days - 1)
}

if ($end -lt $start) { Write-Error "-To is before -From"; exit 1 }

$dates = @()
for ($d = $start; $d -le $end; $d = $d.AddDays(1)) {
    $dates += $d.ToString("yyyyMMdd")
}

Write-Host "==> Generating $($dates.Count) puzzle(s): $($dates[0]) .. $($dates[-1])"
$fail = 0
foreach ($date in $dates) {
    Write-Host "    generate $date ..."
    node generate-daily.js $date
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "generate-daily.js failed for $date"
        $fail++
        continue
    }
}

if ($fail -ne 0) {
    Write-Error "Stopped: $fail generate failure(s). Fix those dates before uploading."
    exit 1
}

if (-not $Upload) {
    Write-Host "`nLocal JSON ready in $PSScriptRoot"
    Write-Host "Inspect the files, then upload with:"
    Write-Host "  .\new-range.ps1 -From $($dates[0]) -To $($dates[-1]) -Upload"
    Write-Host "or:"
    Write-Host "  .\upload-range.ps1 -Dates $($dates -join ',')"
    exit 0
}

Write-Host "`n==> Uploading $($dates.Count) key(s) to KV ..."
foreach ($date in $dates) {
    $file = "$date.json"
    Write-Host "    daily:$date"
    npx wrangler kv key put --binding=BORDY_KV "daily:$date" --path="$file"
    if ($LASTEXITCODE -ne 0) { Write-Warning "wrangler put failed for $date"; $fail++; continue }

    try {
        $r = Invoke-RestMethod -Uri "$WorkerUrl/api/daily/$date.json" -Method GET
        $clues = ($r.givens | Where-Object { $_ -eq $true }).Count
        Write-Host "    OK  givens=$clues/36  edges=$($r.edges.Count)"
    } catch {
        Write-Warning "verify failed for $date (KV may need a few seconds): $_"
        $fail++
    }
}

if ($fail -eq 0) { Write-Host "`nAll daily puzzles are live." }
else { Write-Warning "`nDone with $fail problem(s) — re-run -Upload to retry." }
