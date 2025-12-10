# 🧭 Script đóng gói ứng dụng WPF + sinh version.json

# === Cấu hình mặc định ===
$projectDir  = "$PSScriptRoot\..\src\ezMix\Desktop"
$outputDir   = "$PSScriptRoot\..\output"
$framework   = "net10.0-windows7.0"
$publishDir  = "$projectDir\bin\Release\$framework\publish"

# === Kiểm tra publish directory ===
if (-not (Test-Path $publishDir)) {
    Write-Host "❌ Không tìm thấy thư mục publish: $publishDir"
    return
}

# === Đọc thông tin từ .csproj ===
[xml]$csproj = Get-Content "$projectDir\Desktop.csproj"
$props = $csproj.Project.PropertyGroup | Where-Object { $_.Version }

$appName    = $props.AppName
$appVersion = $props.Version
$gitUser    = $props.GitHubUser
$gitRepo    = $props.GitHubRepo

if (-not $appName -or -not $appVersion) {
    Write-Host "❌ Thiếu AppName hoặc version trong .csproj"
    return
}

# === Chuẩn bị tên và link zip
$zipName     = "$appName-v$appVersion.zip"
$zipPath     = Join-Path $outputDir $zipName
$zipUrl      = "https://github.com/$gitUser/$gitRepo/releases/download/v$appVersion/$zipName"
$versionUrl  = "https://raw.githubusercontent.com/$gitUser/$gitRepo/main/output/version.json"

# === Tạo dữ liệu version.json
$buildTime = Get-Date -Format "yyyy-MM-dd HH:mm"
$sha = (git rev-parse --short HEAD) -replace "`n", ""
if (-not $sha) { $sha = "dev" }

$meta = [PSCustomObject]@{
    AppName     = $appName
    Version     = $appVersion
    File        = $zipName
    ZipUrl      = $zipUrl
    VersionUrl  = $versionUrl
    GitHubUser  = $gitUser
    GitHubRepo  = $gitRepo
    Build       = $buildTime
    Sha         = $sha
    ChangeLog   = "- Build Release lúc $buildTime"
}

# === Ghi version.json vào thư mục publish
$versionPath = Join-Path $publishDir "version.json"
$meta | ConvertTo-Json -Depth 3 | Set-Content -Encoding UTF8 -Path $versionPath

# === Đảm bảo thư mục output
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

# === Nén toàn bộ thư mục publish (gồm cả Version.json)
Compress-Archive -Path "$publishDir\*" -DestinationPath $zipPath -Force

# === Copy Version.json về thư mục BuildOutput
Copy-Item -Path $versionPath -Destination (Join-Path $outputDir "version.json") -Force

# === Hiển thị kết quả
Write-Host "🎉 ĐÓNG GÓI THÀNH CÔNG!"
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
Write-Host "📦 Tên gói      : $zipName"
Write-Host "📂 Thư mục lưu  : $outputDir"
Write-Host "📄 Bao gồm      : Build + version.json"
Write-Host "🔗 Link tải     : $zipUrl"
Write-Host "🔖 Phiên bản    : $appVersion ($sha)"
Write-Host "🕒 Thời điểm    : $buildTime"
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`n"
