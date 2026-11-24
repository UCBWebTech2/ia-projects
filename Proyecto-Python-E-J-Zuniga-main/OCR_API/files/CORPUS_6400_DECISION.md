# Decisión: corpus 6400

## Estado verificado

- El corpus original/exportado de **6400** muestras del modelo de producción **no está presente en el repositorio** con provenance verificable.
- Existe una reconstrucción trazable: `OCR_API/files/corpus_reconstruido_provenance.csv` + `OCR_API/files/corpus_reconstruido_provenance.manifest.json`.
- La reconstrucción deduplica por `sha256(label + "\n" + texto_normalizado)` desde fuentes disponibles (`train_4000.csv`, `test_4000.csv`, `test_dataset.csv`, `dataset_derecho.csv`, `dataset_derecho_4k.csv`).
- Resultado de reconstrucción: **5767 filas únicas** (penal 3051, civil 2716), con solapamientos documentados en el manifiesto.

## Decisión

**Aceptar la reconstrucción trazable como evidencia limitada, no como sustituto del corpus original.**

## Justificación

1. **Trazabilidad disponible**: la reconstrucción tiene hashes, fuentes, conteos y advertencia explícita; permite auditoría.
2. **No equivalencia probada**: 5767 únicos != 6400 originales; no hay evidencia de que la distribución, preprocesamiento o etiquetas sean idénticos al corpus de producción.
3. **Riesgo de defensa**: presentar la reconstrucción como "el corpus original" sería incorrecto. Presentarla como "reconstrucción trazable con limitaciones" es defendible si se declara explícitamente.

## Condición para cerrar la brecha

Para reclamar reproducibilidad completa del modelo de producción, aportar uno de estos artefactos con provenance verificable:

- Export del corpus original (6400) con `fuente, fecha, id_doc, hash, etiqueta, split`.
- Snapshot inmutable del dataset usado en entrenamiento (con hash del archivo y comando/commit de generación).
- Manifiesto firmado/versionado que mapee las 6400 muestras a su origen.

Mientras no exista, toda evaluación sobre la reconstrucción debe etiquetarse como **"evidencia limitada / reconstruida"**.
