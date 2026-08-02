<#
  upload-daily.ps1 — 把某天的每日题目 JSON 上传到 Cloudflare KV 并验证。

  用法（在 cloudflare/bordy-api 目录下）:
    .\upload-daily.ps1                      # 用今天(UTC)的日期，读取 <日期>.json
    .\upload-daily.ps1 -Date 20260803       # 指定日期，读取 20260803.json
    .\upload-daily.ps1 -Date 20260803 -File .\mypuzzle.json
    .\upload-daily.ps1 -Deploy              # 顺便先部署 Worker（改过 Worker 代码时才需要）

  题目 JSON 怎么来：Unity 菜单 Bordy -> Export Daily Template JSON 会导出一份，
  改名成 <日期>.json 即可；或按同样格式手写。
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

# 1) 文件存在 + JSON 合法性检查
if (-not (Test-Path $File)) {
    Write-Error "找不到题目文件: $File`n请先在 Unity 里用 Bordy -> Export Daily Template JSON 导出，改名为 $Date.json；或用 -File 指定路径。"
    exit 1
}
Write-Host "==> 校验 JSON: $File"
try {
    $dto = Get-Content $File -Raw -Encoding UTF8 | ConvertFrom-Json
    if (-not $dto.solution -or -not $dto.givens) { throw "缺少 solution / givens 字段" }
} catch {
    Write-Error "JSON 不合法: $_"
    exit 1
}

# 2) 可选：部署 Worker（仅在改过 Worker 代码时需要）
if ($Deploy) {
    Write-Host "==> 部署 Worker ..."
    npx wrangler deploy
}

# 3) 上传到 KV，键名 daily:<日期>
Write-Host "==> 上传到 KV: daily:$Date  (来源 $File)"
npx wrangler kv key put --binding=BORDY_KV "daily:$Date" --path="$File" --remote
if ($LASTEXITCODE -ne 0) { Write-Error "wrangler kv put 失败"; exit 1 }

# 4) 验证线上能取到
$url = "$WorkerUrl/api/daily/$Date.json"
Write-Host "==> 验证 $url"
try {
    $resp = Invoke-RestMethod -Uri $url -Method GET
    $edgeCount = if ($resp.edges) { $resp.edges.Count } else { 0 }
    Write-Host "OK ✅  date=$($resp.date) size=$($resp.size) edges=$edgeCount"
    Write-Host "每日题目已上线: daily:$Date"
} catch {
    Write-Error "验证失败（KV 可能还没生效，稍等重试）: $_"
    exit 1
}
