# Build UI in Docker (avoids local NuGet restore on C:)
docker build -t ejz-ui:local -f UI-Sistema-de-Administracion-documental-E-J-Zuniga-master/Dockerfile UI-Sistema-de-Administracion-documental-E-J-Zuniga-master
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "UI image built: ejz-ui:local"
