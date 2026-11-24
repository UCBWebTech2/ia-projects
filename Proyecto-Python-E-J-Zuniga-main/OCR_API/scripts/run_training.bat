@echo off
echo Iniciando entrenamiento automatico...
cd /d "%~dp0\..\.."

"%CD%\venv\Scripts\python.exe" -m OCR_API.scripts.entrenar_modelo

echo Entrenamiento finalizado.
pause
