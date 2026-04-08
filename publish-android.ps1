param(
    [string]$Framework = "net10.0-android",
    [string]$Configuration = "Release",
    [string]$PackageFormat = "apk"
)

$ErrorActionPreference = 'Stop'

Set-Location $PSScriptRoot

if (-not $env:JAVA_HOME -or -not (Test-Path $env:JAVA_HOME)) {
    $defaultJavaHome = 'C:\Program Files\Android\Android Studio\jbr'
    if (Test-Path $defaultJavaHome) {
        $env:JAVA_HOME = $defaultJavaHome
    }
}

if (-not $env:ANDROID_SDK_ROOT -or -not (Test-Path $env:ANDROID_SDK_ROOT)) {
    $defaultAndroidSdk = Join-Path $env:LOCALAPPDATA 'Android\Sdk'
    if (Test-Path $defaultAndroidSdk) {
        $env:ANDROID_SDK_ROOT = $defaultAndroidSdk
    }
}

if (-not $env:JAVA_HOME -or -not (Test-Path $env:JAVA_HOME)) {
    throw "JAVA_HOME não foi encontrado. Ajuste para um JDK válido antes de rodar o script."
}

if (-not $env:ANDROID_SDK_ROOT -or -not (Test-Path $env:ANDROID_SDK_ROOT)) {
    throw "ANDROID_SDK_ROOT não foi encontrado. Ajuste para o SDK do Android antes de rodar o script."
}

$projectFile = Join-Path $PSScriptRoot 'Avalon.csproj'
$publishDir = Join-Path $PSScriptRoot "bin\$Configuration\$Framework\publish"

$env:PATH = "$env:JAVA_HOME\bin;$env:ANDROID_SDK_ROOT\platform-tools;$env:ANDROID_SDK_ROOT\emulator;$env:PATH"

Write-Host "=== Publicação Android ===" -ForegroundColor Cyan
Write-Host "Projeto: $projectFile"
Write-Host "Framework: $Framework"
Write-Host "Configuração: $Configuration"
Write-Host "Formato: $PackageFormat"
Write-Host "JAVA_HOME: $env:JAVA_HOME"
Write-Host "ANDROID_SDK_ROOT: $env:ANDROID_SDK_ROOT"
Write-Host ""

if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}

dotnet publish $projectFile -f $Framework -c $Configuration `
    -p:AndroidPackageFormat=$PackageFormat `
    -p:JavaSdkDirectory="$env:JAVA_HOME" `
    -p:AndroidSdkDirectory="$env:ANDROID_SDK_ROOT"

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "Publicação concluída." -ForegroundColor Green
Write-Host "Pasta de saída: $publishDir"

$packages = Get-ChildItem -Path $publishDir -Recurse -Include *.apk, *.aab -ErrorAction SilentlyContinue

if ($packages) {
    Write-Host ""
    Write-Host "Pacotes gerados:" -ForegroundColor Green
    $packages | ForEach-Object { Write-Host "- $($_.FullName)" }
}
else {
    Write-Host "Nenhum .apk/.aab encontrado ainda na pasta de publish." -ForegroundColor Yellow
}
