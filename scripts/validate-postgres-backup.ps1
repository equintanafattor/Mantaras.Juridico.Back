[CmdletBinding()]
param(
  [Parameter()]
  [string]$BackupDirectory = (Get-Location).Path
)

$ErrorActionPreference = "Stop"

$containerName = $null
$decryptedFile = $null
$secureBackupPassword = $null
$plainBackupPassword = $null

try {
  if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw "Docker no se encuentra instalado o no está disponible en PATH."
  }

  docker info *> $null

  if ($LASTEXITCODE -ne 0) {
    throw "Docker Desktop no está iniciado o el motor de contenedores Linux no está disponible."
  }

  $backupDirectoryPath = (
    Resolve-Path -LiteralPath $BackupDirectory
  ).Path

  $encryptedFiles = @(
    Get-ChildItem `
      -LiteralPath $backupDirectoryPath `
      -File `
      -Filter "*.dump.enc"
  )

  if ($encryptedFiles.Count -eq 0) {
    throw "No se encontró ningún archivo .dump.enc en '$backupDirectoryPath'."
  }

  if ($encryptedFiles.Count -gt 1) {
    $fileNames = $encryptedFiles.Name -join ", "

    throw "Se encontró más de un backup cifrado. Dejá solamente el que querés validar: $fileNames"
  }

  $encryptedFile = $encryptedFiles[0]

  $hashFilePath = "$($encryptedFile.FullName).sha256"

  if (-not (Test-Path -LiteralPath $hashFilePath)) {
    throw "No se encontró el archivo de integridad '$($encryptedFile.Name).sha256'."
  }

  Write-Host "Verificando integridad SHA-256..."

  $hashFileContent = (
    Get-Content `
      -LiteralPath $hashFilePath `
      -Raw
  ).Trim()

  $expectedHash = (
    $hashFileContent -split "\s+"
  )[0].ToLowerInvariant()

  $actualHash = (
    Get-FileHash `
      -LiteralPath $encryptedFile.FullName `
      -Algorithm SHA256
  ).Hash.ToLowerInvariant()

  if ($actualHash -ne $expectedHash) {
    throw "La suma SHA-256 no coincide. El backup podría estar corrupto o incompleto."
  }

  Write-Host "Integridad SHA-256 verificada correctamente."
  Write-Host ""

  $secureBackupPassword = Read-Host `
    "Contraseña de cifrado del backup" `
    -AsSecureString

  $plainBackupPassword = [System.Net.NetworkCredential]::new(
    "",
    $secureBackupPassword
  ).Password

  if ([string]::IsNullOrWhiteSpace($plainBackupPassword)) {
    throw "La contraseña de cifrado no puede estar vacía."
  }

  $runId = [Guid]::NewGuid().ToString("N").Substring(0, 12)

  $decryptedFileName = "database-restored-$runId.dump"
  $decryptedFile = Join-Path `
    $backupDirectoryPath `
    $decryptedFileName

  $containerName = "mantaras-postgres-restore-$runId"

  $env:BACKUP_ENCRYPTION_PASSWORD = $plainBackupPassword

  Write-Host ""
  Write-Host "Descifrando backup..."

  docker run --rm `
    --volume "${backupDirectoryPath}:/backup" `
    --env BACKUP_ENCRYPTION_PASSWORD `
    --env "ENCRYPTED_FILE=$($encryptedFile.Name)" `
    --env "DECRYPTED_FILE=$decryptedFileName" `
    alpine:3.22 `
    sh -ceu '
      apk add --no-cache openssl >/dev/null

      openssl enc \
        -d \
        -aes-256-cbc \
        -pbkdf2 \
        -iter 200000 \
        -in "/backup/$ENCRYPTED_FILE" \
        -out "/backup/$DECRYPTED_FILE" \
        -pass env:BACKUP_ENCRYPTION_PASSWORD
    '

  if ($LASTEXITCODE -ne 0) {
    throw "No se pudo descifrar el backup. Verificá la contraseña."
  }

  if (-not (Test-Path -LiteralPath $decryptedFile)) {
    throw "No se generó el archivo descifrado."
  }

  $decryptedFileInfo = Get-Item -LiteralPath $decryptedFile

  if ($decryptedFileInfo.Length -eq 0) {
    throw "El archivo descifrado está vacío."
  }

  Write-Host "Backup descifrado correctamente."
  Write-Host ""
  Write-Host "Iniciando PostgreSQL temporal..."

  docker run --detach `
    --name $containerName `
    --env POSTGRES_DB=restore_validation `
    --env POSTGRES_USER=postgres `
    --env POSTGRES_PASSWORD=restore_validation_2026 `
    --volume "${backupDirectoryPath}:/backup:ro" `
    postgres:18-alpine |
    Out-Null

  if ($LASTEXITCODE -ne 0) {
    throw "No se pudo iniciar PostgreSQL temporal."
  }

  $databaseReady = $false

  for ($attempt = 1; $attempt -le 30; $attempt++) {
    docker exec `
      $containerName `
      pg_isready `
      --username=postgres `
      --dbname=restore_validation |
      Out-Null

    if ($LASTEXITCODE -eq 0) {
      $databaseReady = $true
      break
    }

    Start-Sleep -Seconds 2
  }

  if (-not $databaseReady) {
    throw "PostgreSQL temporal no quedó disponible."
  }

  Write-Host "PostgreSQL temporal disponible."
  Write-Host ""
  Write-Host "Restaurando backup..."

  docker exec `
    $containerName `
    pg_restore `
    --username=postgres `
    --dbname=restore_validation `
    --no-owner `
    --no-privileges `
    --exit-on-error `
    "/backup/$decryptedFileName"

  if ($LASTEXITCODE -ne 0) {
    throw "Falló la restauración del backup."
  }

  Write-Host "Backup restaurado correctamente."
  Write-Host ""
  Write-Host "Validando información restaurada..."

  $validationSql = @'
DO $validation$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM "__EFMigrationsHistory"
  ) THEN
    RAISE EXCEPTION 'La base restaurada no contiene migraciones de EF Core.';
  END IF;

  IF NOT EXISTS (
    SELECT 1
    FROM "AspNetUsers"
  ) THEN
    RAISE EXCEPTION 'La base restaurada no contiene usuarios.';
  END IF;
END
$validation$;

SELECT
  (SELECT COUNT(*) FROM "Clientes") AS clientes,
  (SELECT COUNT(*) FROM "Casos") AS casos,
  (SELECT COUNT(*) FROM "Expedientes") AS expedientes,
  (SELECT COUNT(*) FROM "AspNetUsers") AS usuarios,
  (
    SELECT COUNT(*)
    FROM "__EFMigrationsHistory"
  ) AS migraciones;
'@

  $validationSql |
    docker exec `
      --interactive `
      $containerName `
      psql `
      --username=postgres `
      --dbname=restore_validation `
      --set=ON_ERROR_STOP=1 `
      --file=-

  if ($LASTEXITCODE -ne 0) {
    throw "Falló la validación de los datos restaurados."
  }

  Write-Host ""
  Write-Host "Restauración manual validada correctamente."
}
finally {
  Write-Host ""
  Write-Host "Eliminando recursos temporales..."

  if ($containerName) {
    docker rm --force $containerName 2>$null |
      Out-Null
  }

  if (
    $decryptedFile -and
    (Test-Path -LiteralPath $decryptedFile)
  ) {
    Remove-Item `
      -LiteralPath $decryptedFile `
      -Force
  }

  Remove-Item `
    Env:BACKUP_ENCRYPTION_PASSWORD `
    -ErrorAction SilentlyContinue

  $plainBackupPassword = $null
  $secureBackupPassword = $null

  Remove-Variable `
    plainBackupPassword `
    -ErrorAction SilentlyContinue

  Remove-Variable `
    secureBackupPassword `
    -ErrorAction SilentlyContinue

  Write-Host "Limpieza finalizada."
}