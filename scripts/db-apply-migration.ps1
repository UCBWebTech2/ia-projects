param(
  [string]$SqlFile = "APIsistema-administracion-documental-ej-zuniga-main/Database/002_fase2_feedback_model_registry.sql",
  [string]$Service = "db",
  [string]$DbName = $env:DB_NAME
)
$ErrorActionPreference = "Stop"
if (-not $DbName) { $DbName = "BDProyecto" }
$sa = $env:SA_PASSWORD
if (-not $sa) { $sa = "YourStrong!Passw0rd" }

if (-not (Test-Path $SqlFile)) {
  throw "No existe el archivo SQL: $SqlFile"
}

$leaf = Split-Path $SqlFile -Leaf
$remote = "/docker-entrypoint-initdb/$leaf"
Write-Host "Aplicando $SqlFile en servicio $Service db=$DbName ..."
docker exec -i ejz-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$sa" -C -d $DbName -i $remote
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "Migración aplicada: $leaf"
