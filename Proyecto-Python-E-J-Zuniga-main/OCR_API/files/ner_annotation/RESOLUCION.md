# Resolución de anotación NER

## 1) Anotación doble + kappa

- **A1 (pre-corrección)**: `corpus_a1.jsonl` — borrador original con 40 spans misalineados (estado `DRAFT_NEEDS_HUMAN_REVIEW`).
- **A2 (post-revisión/corrección)**: `corpus_a2.jsonl` — entidades corregidas, cobertura de etiquetas completada, estado `REVIEWED_DRAFT`.
- **Resuelto**: `corpus_resolved.jsonl` — versión adjudicada tras resolver desacuerdos.
- **Kappa**: `kappa_report.json`, calculado con `OCR_API/scripts/kappa_ner.py`.

Resultado observado (A1 vs A2):

```json
{"ids_compared": 36, "kappa": -0.721518987341772, "f1_micro": 0.2608695652173913, "disagreements": 30}
```

Interpretación: el kappa negativo/F1 bajo refleja la corrección masiva de spans misalineados y entidades faltantes en el borrador original. No es una medida de desacuerdo entre dos anotadores humanos independientes; es la distancia entre borrador automático y versión revisada. **Para una defensa metodológica fuerte se requiere una segunda pasada humana independiente sobre `corpus_resolved.jsonl` (o sobre un nuevo lote) y recalcular kappa entre dos anotadores humanos.**

## 2) Corpus defendible

- Corregidos los **40 spans misalineados** detectados por `validar_corpus_ner.py`.
- Validación actual: `corpus_draft.validation.json` → `issues: 0`, `36` oraciones, `75` entidades.
- Cobertura por etiqueta objetivo completada: `PER, LOC, ORG, ARTICULO, LEY, INSTITUCION, INSTRUMENTO`.
- Se agregaron 5 filas de cobertura (`cov-0001`..`cov-0005`) para asegurar presencia de `INSTRUMENTO` e `INSTITUCION` y reforzar `LEY/ARTICULO`.
- Partición 70/15/15 regenerada con seed 42 en `splits/`: `train=25, dev=5, test=6`. Ver `splits/split_manifest.json`.

## 3) Limitaciones honestas

- El corpus sigue siendo pequeño (36 oraciones). Es útil como baseline/control de calidad, no como evidencia estadística robusta de generalización.
- Las etiquetas fueron corregidas/revisadas en este flujo asistido; falta adjudicación humana independiente para reclamo de IAA defendible.
- El texto contiene mojibake heredado del origen (`training_ner_data.py`); se preservó para mantener trazabilidad con offsets, pero debe normalizarse en una versión canónica si se usa para entrenamiento productivo.
- `corpus_draft.jsonl` es el archivo canónico corregido; `corpus_resolved.jsonl` es copia adjudicada.

## Comandos de reproducción

```powershell
python OCR_API/scripts/validar_corpus_ner.py --input OCR_API/files/ner_annotation/corpus_draft.jsonl --output OCR_API/files/ner_annotation/corpus_draft.validation.json
python OCR_API/scripts/kappa_ner.py --a OCR_API/files/ner_annotation/corpus_a1.jsonl --b OCR_API/files/ner_annotation/corpus_a2.jsonl --output OCR_API/files/ner_annotation/kappa_report.json
python OCR_API/scripts/split_ner_corpus.py --input OCR_API/files/ner_annotation/corpus_draft.jsonl --out-dir OCR_API/files/ner_annotation/splits
```
