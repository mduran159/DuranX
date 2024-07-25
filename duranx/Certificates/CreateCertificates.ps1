# Define variables for passwords and file paths
$password = "duranxpassword"
$certPath = ".\"
$identityServerCertsPath = "C:\Users\User\Documents\NetProjects\duranx\Identity\IdentityServer\Certificates\"
$inventoryCertsPath = "C:\Users\User\Documents\NetProjects\duranx\Services\Inventory\Inventory.API\Certificates\"



# Define paths for generated files
$identityserverKey = Join-Path $certPath "identityserverSigning.key"
$identityserverCsr = Join-Path $certPath "identityserverSigning.csr"
$identityserverCrt = Join-Path $certPath "identityserverSigning.crt"
$identityserverPfx = Join-Path $certPath "identityserverSigning.pfx"

$webapiKey = Join-Path $certPath "webapiValidation.key"
$webapiCsr = Join-Path $certPath "webapiValidation.csr"
$webapiCrt = Join-Path $certPath "webapiValidation.crt"
$webapiPfx = Join-Path $certPath "webapiValidation.pfx"

$encryptionKey = Join-Path $certPath "encryption.key"
$encryptionCsr = Join-Path $certPath "encryption.csr"
$encryptionCrt = Join-Path $certPath "encryption.crt"
$encryptionPfx = Join-Path $certPath "encryption.pfx"



# Remove existing files if they exist
if (Test-Path $identityserverKey) { Remove-Item $identityserverKey }
if (Test-Path $identityserverCsr) { Remove-Item $identityserverCsr }
if (Test-Path $identityserverCrt) { Remove-Item $identityserverCrt }
if (Test-Path $identityserverPfx) { Remove-Item $identityserverPfx }

if (Test-Path $webapiKey) { Remove-Item $webapiKey }
if (Test-Path $webapiCsr) { Remove-Item $webapiCsr }
if (Test-Path $webapiCrt) { Remove-Item $webapiCrt }
if (Test-Path $webapiPfx) { Remove-Item $webapiPfx }

if (Test-Path $encryptionKey) { Remove-Item $encryptionKey }
if (Test-Path $encryptionCsr) { Remove-Item $encryptionCsr }
if (Test-Path $encryptionCrt) { Remove-Item $encryptionCrt }
if (Test-Path $encryptionPfx) { Remove-Item $encryptionPfx }



# Generate IdentityServer key and certificate
Write-Output "Generating IdentityServer key..."
& openssl genpkey -algorithm RSA -out $identityserverKey -aes256 -pass pass:$password
Write-Output "Generating IdentityServer CSR..."
& openssl req -new -key $identityserverKey -out $identityserverCsr -subj "/C=CR/ST=Alajuela/L=Alajuela/O=duranx/OU=duranx/CN=identityserverSigning/emailAddress=maicoldv@gmail.com" -passin pass:$password
Write-Output "Generating IdentityServer certificate..."
& openssl x509 -req -days 365 -in $identityserverCsr -signkey $identityserverKey -out $identityserverCrt -passin pass:$password
Write-Output "Generating IdentityServer PFX..."
& openssl pkcs12 -export -out $identityserverPfx -inkey $identityserverKey -in $identityserverCrt -passout pass:$password

# Generate WebAPI key and certificate
Write-Output "Generating WebAPI key..."
& openssl genpkey -algorithm RSA -out $webapiKey -aes256 -pass pass:$password
Write-Output "Generating WebAPI CSR..."
& openssl req -new -key $webapiKey -out $webapiCsr -subj "/C=CR/ST=Alajuela/L=Alajuela/O=duranx/OU=duranx/CN=webapiValidation/emailAddress=maicoldv@gmail.com" -passin pass:$password
Write-Output "Generating WebAPI certificate..."
& openssl x509 -req -days 365 -in $webapiCsr -signkey $webapiKey -out $webapiCrt -passin pass:$password
Write-Output "Generating WebAPI PFX..."
& openssl pkcs12 -export -out $webapiPfx -inkey $webapiKey -in $webapiCrt -passout pass:$password

# Generate Encryption key and certificate
Write-Output "Generating Encryption key..."
& openssl genpkey -algorithm RSA -out $encryptionKey -aes256 -pass pass:$password
Write-Output "Generating Encryption CSR..."
& openssl req -new -key $encryptionKey -out $encryptionCsr -subj "/C=CR/ST=Alajuela/L=Alajuela/O=duranx/OU=duranx/CN=encryption/emailAddress=maicoldv@gmail.com" -passin pass:$password
Write-Output "Generating Encryption certificate..."
& openssl x509 -req -days 365 -in $encryptionCsr -signkey $encryptionKey -out $encryptionCrt -passin pass:$password
Write-Output "Generating Encryption PFX..."
& openssl pkcs12 -export -out $encryptionPfx -inkey $encryptionKey -in $encryptionCrt -passout pass:$password

Write-Output "Certificados generados exitosamente."



#Copiar dentro de los proyectos.
# Ensure target directories exist
if (-not (Test-Path $identityServerCertsPath)) {
    New-Item -Path $identityServerCertsPath -ItemType Directory
}
if (-not (Test-Path $inventoryCertsPath)) {
    New-Item -Path $inventoryCertsPath -ItemType Directory
}

# Copy generated certificates to the specified directories
Get-ChildItem -Path $generatedCertsPath -File | Where-Object { $_.Name -notmatch "CreateCertificates.ps1" } | ForEach-Object {
    Copy-Item -Path $_.FullName -Destination $identityServerCertsPath -Force
    Copy-Item -Path $_.FullName -Destination $inventoryCertsPath -Force
}

Write-Output "Certificados copiados exitosamente a las carpetas especificadas."



# Import PFX files into Windows certificate store
Write-Output "Importing certificates into Windows certificate store..."

# Import certificates
$certificates = @($identityserverPfx, $webapiPfx, $encryptionPfx)
foreach ($certificate in $certificates) {
    Write-Output "Importing $certificate..."
    $securePassword = ConvertTo-SecureString -String $password -Force -AsPlainText
    Import-PfxCertificate -FilePath $certificate -CertStoreLocation "Cert:\LocalMachine\My" -Password $securePassword
}

Write-Output "Certificados importados al almacen de certificados de Windows exitosamente."
