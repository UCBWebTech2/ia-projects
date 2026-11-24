# Runs NER baseline inside OCR container/image context.
# Uses validated corpus split (post-correction) instead of invalid training_ner_data.py.
param(
  [string]$Data = "OCR_API/files/ner_annotation/splits/test.jsonl",
  [string]$Model = "es_core_news_lg",
  [string]$Output = "OCR_API/files/ner_annotation/splits/test.ner_eval.json"
)
$ErrorActionPreference = "Stop"

Write-Host "Running NER baseline: data=$Data model=$Model output=$Output"
docker compose run --rm ocr-api python OCR_API/scripts/evaluar_ner.py --data $Data --model $Model --output $Output
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "NER baseline report: $Output"
