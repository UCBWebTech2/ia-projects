# Sistema de Administración Documental - Estudio Jurídico Zúñiga

Workspace con los componentes del proyecto de grado de **Miguel Ángel Rocabado Valencia** (caso de estudio: Estudio Jurídico Zúñiga, La Paz, Bolivia).

## Componentes

| Carpeta | Descripción | Stack |
|---|---|---|
| `APIsistema-administracion-documental-ej-zuniga-main/` | API principal de negocio, autenticación, documentos, auditoría e integración con OCR | .NET 8, EF Core, SQL Server, Supabase |
| `Proyecto-Python-E-J-Zuniga-main/` | API de OCR, extracción de entidades y clasificación | Python, FastAPI, Tesseract/PaddleOCR, spaCy, scikit-learn |
| `UI-Sistema-de-Administracion-documental-E-J-Zuniga-master/` | Interfaz web | ASP.NET Core MVC (.NET 8) |
| `legacy/` | Componentes archivados/snapshots históricos (no usar en despliegue activo) | - |

> La carpeta Python vigente para OCR/clasificación es `Proyecto-Python-E-J-Zuniga-main/`. `ProyectoPythonAPI-main` fue archivada en `legacy/` durante la Fase 0.

## Despliegue con Docker

Ver [`README.Docker.md`](README.Docker.md) y [`PLAN_CORRECCION_AUDITORIA.md`](PLAN_CORRECCION_AUDITORIA.md).

Pasos rápidos:

```bash
cp .env.example .env
# generar secretos JWT internos en secrets/
docker compose up --build -d
```

Puertos por defecto: UI `5290`, API .NET `5159`, OCR `8000`, SQL Server `1433`.

## Desarrollo local

### API .NET

```powershell
cd APIsistema-administracion-documental-ej-zuniga-main/SendEmail
dotnet restore
dotnet run
```

Configurar secretos/connection strings mediante User Secrets o variables de entorno (ver `CONFIGURACION.md`).

### OCR Python

```powershell
cd Proyecto-Python-E-J-Zuniga-main
python -m venv .venv
.venv\Scripts\Activate.ps1
pip install -r requirements.txt
uvicorn OCR_API.app.main:app --reload --port 8000
```

### Frontend

```powershell
cd UI-Sistema-de-Administracion-documental-E-J-Zuniga-master/PFEstudioJuridico
dotnet restore
dotnet run
```

## Auditoría y plan de corrección

- Auditoría fuente: `resultados_auditoria.xlsx`
- Plan: [`PLAN_CORRECCION_AUDITORIA.md`](PLAN_CORRECCION_AUDITORIA.md)

### Fase 0 ejecutada (cambios técnicos iniciales)

- Corregido import NER `TRAINING_DATA` en `OCR_API/scripts/entrenar_ner.py`.
- Whitelist NER en .NET incluye `INSTITUCION` y `ORG`.
- Eliminada constante muerta `OcrOperations.PrepararDataset`.
- Archivado `ProyectoPythonAPI-main` en `legacy/`.
- Eliminado `WeatherForecastController`/modelo plantilla.
- `append_dataset_rows` con ruta anclada a settings, validación, dedupe por hash, backup y escritura atómica.
- Recarga de modelo en script con `OCR_ADMIN_TOKEN` opcional; sin POST no autenticado silencioso.
- `/ProbarClasificador` protegido con JWT interno admin.
- Eliminado `ModelRegistry` muerto del servicio de clasificación vigente.
- Snapshots de requirements movidos a `legacy/requirements_snapshots/`.

## Notas importantes

- El modelo/artefactos ML tienen hallazgos críticos de reproducibilidad y métricas documentados en la auditoría; no asumir métricas de la tesis sin reproducir.
- El esquema SQL en `APIsistema-administracion-documental-ej-zuniga-main/Database/001_schema.sql` es base para Docker y debe validarse contra el esquema real.
- No commitear secretos (`.env`, PEM, claves). Use `secrets/README.md`.