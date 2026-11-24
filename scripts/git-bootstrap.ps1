param(
  [string]$RemoteUrl = "",
  [string]$Branch = "main",
  [string]$Message = "chore: baseline docker + phase 0-4 code updates"
)
$ErrorActionPreference = "Stop"

if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
  throw "git no está instalado o no está en PATH."
}

if (-not (Test-Path .git)) {
  git init
  git branch -M $Branch
}

git status --short
$changes = git status --porcelain
if (-not $changes) {
  Write-Host "No hay cambios para commitear."
} else {
  Write-Host "Staging cambios (revisa .gitignore antes de commitear)..."
  git add -A
  git status --short
  git commit -m $Message
}

if ($RemoteUrl) {
  $existing = git remote get-url origin 2>$null
  if ($LASTEXITCODE -eq 0 -and $existing) {
    Write-Host "Remote origin ya existe: $existing"
  } else {
    git remote add origin $RemoteUrl
    Write-Host "Remote origin agregado: $RemoteUrl"
  }
  git branch -M $Branch
  Write-Host "Push manual cuando esté listo: git push -u origin $Branch"
} else {
  Write-Host "Sin RemoteUrl. Configura remote cuando esté listo."
}
