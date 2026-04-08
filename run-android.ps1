param(
    [string]$AvdName = "Avalon_API_36",
    [string]$Framework = "net10.0-android"
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

$emulatorExe = Join-Path $env:ANDROID_SDK_ROOT 'emulator\emulator.exe'
$adbExe = Join-Path $env:ANDROID_SDK_ROOT 'platform-tools\adb.exe'
$projectFile = Join-Path $PSScriptRoot 'Avalon.csproj'

if (-not (Test-Path $emulatorExe)) {
    throw "Emulator não encontrado em '$emulatorExe'."
}

if (-not (Test-Path $adbExe)) {
    throw "ADB não encontrado em '$adbExe'."
}

$env:PATH = "$env:JAVA_HOME\bin;$env:ANDROID_SDK_ROOT\platform-tools;$env:ANDROID_SDK_ROOT\emulator;$env:PATH"

Write-Host "=== Ambiente Android ===" -ForegroundColor Cyan
Write-Host "JAVA_HOME: $env:JAVA_HOME"
Write-Host "ANDROID_SDK_ROOT: $env:ANDROID_SDK_ROOT"
Write-Host "AVD: $AvdName"
Write-Host ""

$availableAvds = & $emulatorExe -list-avds
if (-not ($availableAvds -contains $AvdName)) {
    Write-Host "AVD '$AvdName' não encontrado." -ForegroundColor Yellow
    Write-Host "Abra o Android Studio > Device Manager e crie um emulador, ou rode: emulator -list-avds" -ForegroundColor Yellow
    exit 1
}

$runningDevice = & $adbExe devices | Select-String 'emulator-\d+\s+device'
if (-not $runningDevice) {
    Write-Host "Iniciando o emulador '$AvdName'..." -ForegroundColor Cyan
    Start-Process -FilePath $emulatorExe -ArgumentList "-avd $AvdName"

    Write-Host "Aguardando o Android concluir o boot..." -ForegroundColor Cyan
    & $adbExe wait-for-device | Out-Null

    do {
        Start-Sleep -Seconds 2
        $bootCompleted = (& $adbExe shell getprop sys.boot_completed 2>$null).Trim()
    } until ($bootCompleted -eq '1')
}
else {
    Write-Host "Já existe um emulador Android online." -ForegroundColor Green
}

Write-Host ""
Write-Host "Executando o app MAUI no Android..." -ForegroundColor Cyan

dotnet build $projectFile -t:Run -f $Framework `
    -p:JavaSdkDirectory="$env:JAVA_HOME" `
    -p:AndroidSdkDirectory="$env:ANDROID_SDK_ROOT"

if ($LASTEXITCODE -eq 0) {
    Write-Host "" 
    Write-Host "App iniciado com sucesso no emulador." -ForegroundColor Green
}
else {
    exit $LASTEXITCODE
}
