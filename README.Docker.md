# Despliegue con Docker

Este repositorio contiene 4 componentes principales:

- **db**: SQL Server 2022.
- **api**: API .NET 8 (`APIsistema-administracion-documental-ej-zuniga-main/SendEmail`).
- **ocr-api**: API Python/FastAPI de OCR y clasificación (`Proyecto-Python-E-J-Zuniga-main/OCR_API`).
- **frontend**: UI ASP.NET Core MVC (`UI-Sistema-de-Administracion-documental-E-J-Zuniga-master/PFEstudioJuridico`).

## Requisitos

- Docker Desktop / Docker Engine con Docker Compose v2.
- Claves JWT internas para comunicación API <-> OCR.

## Configuración rápida

1. Copiar variables de entorno:

   ```bash
   cp .env.example .env
   ```

2. Generar claves internas en `secrets/`:

   ```bash
   openssl genrsa -out secrets/internal-jwt-private-dev.pem 2048
   openssl rsa -in secrets/internal-jwt-private-dev.pem -pubout -out secrets/internal-jwt-public.pem
   ```

3. Revisar `.env` y cambiar al menos:

   - `SA_PASSWORD`
   - `JWT_KEY`
   - credenciales de correo/Supabase/Firebase si aplica

4. Levantar servicios:

   ```bash
   docker compose up --build -d
   ```

## Puertos por defecto

- Frontend: http://localhost:5290
- API .NET: http://localhost:5159/swagger
- OCR API: http://localhost:8000/docs
- SQL Server: localhost:1433

## Inicialización de base de datos

El servicio `db-init` crea la base `DB_NAME` y ejecuta los scripts en `APIsistema-administracion-documental-ej-zuniga-main/Database/*.sql`.

Se incluye `001_schema.sql` como esquema base para despliegue Docker. Si ya tiene una base existente, revise compatibilidad antes de aplicarlo.

## Mejoras aplicadas

- Dockerfiles para API .NET, OCR Python y frontend.
- `docker-compose.yml` con red, volúmenes, healthcheck de SQL Server e inicialización.
- `.env.example` para configuración por entorno.
- CORS configurable en la API .NET (`Cors:AllowedOrigins`).
- Endpoints `/health` en API .NET, OCR y frontend.
- Configuración Firebase del frontend movida a configuración/variables de entorno.
- Configuración de Tesseract en OCR compatible con Linux/contenedores.
- `.dockerignore` para reducir contexto de build.

## Notas

- El proyecto OCR incluye dependencias pesadas (PaddleOCR, spaCy, modelos). La primera construcción puede tardar.
- La carpeta `secrets/` no debe contener claves privadas en control de versiones.
- Para producción, use secretos administrados, TLS, contraseñas fuertes y revise políticas CORS/JWT.
