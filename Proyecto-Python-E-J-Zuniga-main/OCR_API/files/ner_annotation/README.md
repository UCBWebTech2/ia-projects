# Corpus NER (Fase 3)

## Formato JSONL
Una línea por documento/oración:
```json
{"id":"doc-001","text":"...","entities":[[start,end,"LABEL"]],"source":"expediente","annotator":"A1"}
```

## Etiquetas objetivo
`LEY, ARTICULO, INSTRUMENTO, INSTITUCION, PER, LOC, ORG`

## Proceso mínimo
1. Dos anotadores etiquetan el mismo lote.
2. Medir Cohen's kappa por etiqueta y global.
3. Resolver desacuerdos.
4. Validar: `python OCR_API/scripts/validar_corpus_ner.py --input corpus.jsonl`
5. Particionar 70/15/15: `python OCR_API/scripts/split_ner_corpus.py --input corpus.jsonl --out-dir splits`
6. Evaluar baseline: `python OCR_API/scripts/evaluar_ner.py --data splits/test.jsonl --model es_core_news_lg`

## Estado actual (2026-11-22)
- `corpus_draft.jsonl`: corregido y validado (`0 issues`, 36 oraciones, 75 entidades, 7 etiquetas cubiertas).
- Doble revisión registrada: `corpus_a1.jsonl` (pre-corrección), `corpus_a2.jsonl` (post-revisión), `corpus_resolved.jsonl` (adjudicado).
- Kappa/F1 A1 vs A2: ver `kappa_report.json` (refleja corrección de borrador, no IAA humana independiente).
- Partición congelada 70/15/15 en `splits/` (seed 42). Ver `splits/split_manifest.json`.
- **Pendiente para defensa fuerte**: segunda pasada de anotación humana independiente sobre `corpus_resolved.jsonl` y recálculo de kappa entre humanos.
