#Requires -RunAsAdministrator
$ErrorActionPreference = "Stop"

$Repo = Split-Path -Parent $PSScriptRoot
$Sys = Join-Path $Repo "agent\build-driver\driver\Release\akeso_dlp_filter.sys"
$Inf = Join-Path $Repo "agent\driver\akeso_dlp_filter.inf"

if (-not (Test-Path $Sys)) { throw "Driver not built: $Sys" }
if (-not (Test-Path $Inf)) { throw "INF missing: $Inf" }

Write-Host "Enabling test signing..."
bcdedit /set testsigning on | Out-Null

$certName = "AkesoDLP Test"
$cert = Get-ChildItem Cert:\LocalMachine\My | Where-Object { $_.Subject -like "*$certName*" } | Select-Object -First 1
if (-not $cert) {
    Write-Host "Creating test-signing certificate..."
    $cert = New-SelfSignedCertificate -Type CodeSigningCert -Subject "CN=$certName" -CertStoreLocation Cert:\LocalMachine\My -NotAfter (Get-Date).AddYears(5)
    foreach ($storeName in @("Root", "TrustedPublisher")) {
        $store = New-Object System.Security.Cryptography.X509Certificates.X509Store($storeName, "LocalMachine")
        $store.Open("ReadWrite")
        $store.Add($cert)
        $store.Close()
    }
}

$signtool = Get-ChildItem "${env:ProgramFiles(x86)}\Windows Kits\10\bin" -Recurse -Filter "signtool.exe" -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -like "*x64*" } |
    Select-Object -First 1
if ($signtool) {
    Write-Host "Signing driver..."
    & $signtool.FullName sign /v /s My /n $certName /fd SHA256 $Sys
} else {
    Write-Warning "signtool.exe not found — loading unsigned (test signing still required)"
}

$destSys = Join-Path $env:SystemRoot "System32\drivers\akeso_dlp_filter.sys"
Copy-Item $Sys $destSys -Force
Write-Host "Copied $destSys"

Write-Host "Installing INF..."
rundll32.exe setupapi.dll,InstallHinfSection DefaultInstall.NTAMD64 132 $Inf

Write-Host "Loading AkesoDLPFilter..."
fltmc unload AkesoDLPFilter 2>$null
fltmc load AkesoDLPFilter
if ($LASTEXITCODE -ne 0) {
    Write-Warning "fltmc load failed. If test signing was just enabled, reboot and re-run this script."
    sc.exe query AkesoDLPFilter
    exit 1
}

Write-Host "=== fltmc ==="
fltmc
Write-Host "=== instances ==="
fltmc instances
Write-Host "Driver installed and loaded."
