import logging
from pathlib import Path
from typing import Optional

from fastapi import APIRouter, Depends, File, HTTPException, UploadFile

from OCR_API.app.api.upload_validation import IMAGE_TYPES, SUPPORTED_TYPES, read_validated_upload
from OCR_API.app.core.config import settings
from OCR_API.app.core.security import require_internal_token
from OCR_API.app.services.document_service import (
    append_dataset_rows,
    extract_text_from_document,
    extract_text_from_image,
)
from OCR_API.app.services.model_service import preprocess_text
from OCR_API.scripts.EntityRuler import nlp

router = APIRouter()
logger = logging.getLogger("uvicorn.error")


@router.post(
    "/PrepararDataset",
    tags=["Entrenamiento"],
    dependencies=[Depends(require_internal_token("admin", "PrepararDataset"))],
)
async def prepare_dataset(
    file: UploadFile = File(...),
    etiqueta: Optional[str] = None,
):
    try:
        if not etiqueta or not etiqueta.strip():
            raise HTTPException(
                status_code=400,
                detail="Debe proporcionar una etiqueta para el texto.",
            )

        file_content, content_type = await read_validated_upload(file, SUPPORTED_TYPES)
        extracted_text = (
            extract_text_from_image(file_content)
            if content_type in IMAGE_TYPES
            else extract_text_from_document(file_content)
        )
        if not extracted_text or not extracted_text.strip():
            raise HTTPException(
                status_code=422,
                detail="No se pudo extraer texto útil del archivo.",
            )

        doc = nlp(extracted_text.strip())
        rows = []
        for sentence in doc.sents:
            processed = preprocess_text(sentence.text.strip())
            if processed:
                rows.append((processed, etiqueta.strip()))
        if not rows:
            raise HTTPException(
                status_code=422,
                detail="No se generaron segmentos útiles para el dataset.",
            )

        stats = append_dataset_rows(rows)
        return {
            "mensaje": f"{len(rows)} segmentos procesados para el dataset.",
            "archivo_dataset": Path(settings.dataset_path).name,
            "estadisticas": stats,
        }
    except HTTPException:
        raise
    except Exception:
        logger.exception("Error inesperado en /PrepararDataset.")
        raise HTTPException(status_code=500, detail="Error interno al preparar el dataset.")
