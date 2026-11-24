# Operación de pendientes

Estado actualizado: 2026-11-22. Los ítems se resolvieron a nivel de repo/scripts/documentación donde aplica; los que requieren Docker en ejecución, BD existente o decisión humana quedan marcados con su evidencia/límite.

## 1) BD / migración 002 — Resuelto en repo

- `docker-compose.yml` monta `APIsistema-administracion-documental-ej-zuniga-main/Database` en `db-init` y `docker/db-init.sh` aplica `*.sql` en orden al crear/arrancar un entorno limpio.
- `002_fase2_feedback_model_registry.sql` es idempotente (`IF COL_LENGTH` / `IF OBJECT_ID`).
- Para BD existente, usar el helper:

```powershell
./scripts/db-apply-migration.ps1 -SqlFile APIsistema-administracion-documental-ej-zuniga-main/Database/002_fase2_feedback_model_registry.sql
```

O manualmente:

```bash
docker exec -i ejz-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -d BDProyecto -i /docker-entrypoint-initdb/002_fase2_feedback_model_registry.sql
```

Validación esperada: columnas `prediccionIa/confianzaIa/versionModelo/fechaPrediccion/categoriaConfirmadaId/confirmadoPor/fechaConfirmacion` en `dbo.Documentos` y tablas `dbo.ModelRegistry`, `dbo.MapeoCategoria`.

## 2) UI build sin restore local — Resuelto

```powershell
./scripts/docker-ui-build.ps1
```

Construye `ejz-ui:local` dentro de Docker; no requiere restore NuGet en el host. Requiere Docker Desktop/engine.

## 3) Git — Resuelto en repo (push externo pendiente)

```powershell
./scripts/git-bootstrap.ps1 -RemoteUrl "https://github.com/<org>/<repo>.git"
```

- El script inicializa repo si falta, muestra estado, evita commit vacío y no hace push automático.
- `.gitignore` ya excluye secretos, bin/obj, venv, `.env`, y artefactos generados.
- **Pendiente externo**: definir remote real y ejecutar `git push -u origin main` con credenciales/autorización.

## 4) NER baseline — Resuelto en repo

Script actualizado para usar el corpus NER validado:

```powershell
./scripts/ner-baseline-docker.ps1
```

Ejecuta `evaluar_ner.py` sobre `OCR_API/files/ner_annotation/splits/test.jsonl` (fallback documentado en script). Requiere imagen OCR construida (`docker compose build ocr-api` o `docker compose run ...`). No se ejecuta aquí porque Docker no está disponible en este entorno.

## 5) Corpus NER — Resuelto (con limitación IAA humana)

- Corpus corregido/validado: `OCR_API/files/ner_annotation/corpus_draft.jsonl` (`0 issues`, 36 oraciones, 75 entidades, 7 etiquetas).
- Doble revisión registrada: `corpus_a1.jsonl`, `corpus_a2.jsonl`, `corpus_resolved.jsonl`.
- Kappa/F1: `OCR_API/files/ner_annotation/kappa_report.json` (A1 pre-corrección vs A2 post-revisión; no sustituye IAA humana independiente).
- Partición congelada 70/15/15: `OCR_API/files/ner_annotation/splits/` + `split_manifest.json`.
- Detalle: `OCR_API/files/ner_annotation/RESOLUCION.md`.

**Limitación pendiente**: segunda pasada de anotación humana independiente y recálculo de kappa entre humanos para defensa metodológica fuerte.

## 6) Provenance corpus 6400 — Resuelto por decisión documentada

- Decisión: `OCR_API/files/CORPUS_6400_DECISION.md`.
- Reconstrucción trazable aceptada como **evidencia limitada**, no como sustituto del corpus original.
- Artefactos: `OCR_API/files/corpus_reconstruido_provenance.csv`, `OCR_API/files/corpus_reconstruido_provenance.manifest.json` (5767 filas únicas, hashes, fuentes, solapamientos).
- Cierre completo solo con export/snapshot original verificable (6400) o manifiesto equivalente.