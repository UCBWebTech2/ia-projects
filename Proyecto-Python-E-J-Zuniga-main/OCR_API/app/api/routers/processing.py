import logging

from fastapi import APIRouter, Depends, File, HTTPException, Query, UploadFile

from OCR_API.app.api.upload_validation import (
    DOCUMENT_TYPES,
    DOCX_TYPE,
    DOC_TYPE,
    IMAGE_TYPES,
    SUPPORTED_TYPES,
    read_validated_upload,
)
from OCR_API.app.core.security import require_internal_token
from OCR_API.app.services.document_service import (
    extract_text_from_document,
    extract_text_from_image,
    validate_image_conditions,
)
from OCR_API.app.services.entity_service import (
    count_entities,
    extract_entities,
    extract_grouped_entities,
)
from OCR_API.app.services.ocr_service import (
    evaluar_texto_ocr,
    extraer_texto_paddle_desde_imagen,
    extract_text_from_image_auto_detalle,
)

router = APIRouter()
logger = logging.getLogger("uvicorn.error")


def _select_tesseract_text(
    image_bytes: bytes,
    include_generic: bool,
) -> tuple[str, str]:
    normal_text = (extract_text_from_image(image_bytes) or "").strip()
    normal_score = evaluar_texto_ocr(normal_text)
    normal_entities = count_entities(normal_text, include_generic)

    use_auto = (
        not normal_text
        or normal_score < 450
        or len(normal_text) < 800
        or normal_entities == 0
    )
    if not use_auto:
        return normal_text, "tesseract_normal"

    auto_detail = extract_text_from_image_auto_detalle(image_bytes)
    auto_text = (auto_detail["texto"] or "").strip()
    auto_score = auto_detail["scoreTextoCombinado"]
    auto_entities = count_entities(auto_text, include_generic)
    if auto_text and (
        auto_entities > normal_entities
        or auto_score >= normal_score
    ):
        return auto_text, "tesseract_auto"
    return normal_text, "tesseract_normal"


@router.post(
    "/SubirArchivo",
    tags=["ProcesamientoArchivos"],
    dependencies=[Depends(require_internal_token("process", "SubirArchivo"))],
)
async def upload_file(
    file: UploadFile = File(...),
    incluir_genericas: bool = Query(True, alias="incluirGenericas"),
):
    try:
        file_content, content_type = await read_validated_upload(file, SUPPORTED_TYPES)
        recommendations = []

        if content_type in IMAGE_TYPES:
            recommendations = validate_image_conditions(file_content)
            extracted_text, ocr_engine = _select_tesseract_text(
                file_content,
                incluir_genericas,
            )
            score = evaluar_texto_ocr(extracted_text)
            entity_count = count_entities(extracted_text, incluir_genericas)
            if (
                not extracted_text
                or score < 250
                or (len(extracted_text) < 800 and entity_count == 0)
            ):
                raise HTTPException(
                    status_code=422,
                    detail={
                        "mensaje": "La extracción de texto no fue suficientemente confiable.",
                        "recomendacion": "Utilice la extracción avanzada de texto para imágenes difíciles.",
                        "accionSugerida": "USAR_OCR_AVANZADO",
                        "endpointSugerido": "/SubirArchivoPaddle",
                        "motorOCRUsado": ocr_engine,
                        "scoreOCR": round(score, 2),
                        "cantidadCaracteres": len(extracted_text),
                        "cantidadEntidades": entity_count,
                        "recomendacionesImagen": recommendations,
                    },
                )
        else:
            extracted_text = extract_text_from_document(file_content)
            ocr_engine = "document_parser"

        extracted_text = extracted_text.strip()
        return {
            "extracted_text": extracted_text,
            "entities": extract_grouped_entities(extracted_text, incluir_genericas),
            "recomendaciones": recommendations,
            "motorOCRUsado": ocr_engine,
        }
    except HTTPException:
        raise
    except Exception:
        logger.exception("Error inesperado en /SubirArchivo.")
        raise HTTPException(status_code=500, detail="Error interno al procesar el archivo.")


@router.post(
    "/SubirArchivoPaddle",
    tags=["ProcesamientoArchivos"],
    dependencies=[Depends(require_internal_token("process", "SubirArchivoPaddle"))],
)
async def upload_file_paddle(
    file: UploadFile = File(...),
    incluir_genericas: bool = Query(True, alias="incluirGenericas"),
):
    try:
        image_bytes, _ = await read_validated_upload(file, IMAGE_TYPES)
        recommendations = validate_image_conditions(image_bytes)
        detail = extraer_texto_paddle_desde_imagen(image_bytes)
        extracted_text = (detail["texto"] or "").strip()
        if not extracted_text:
            raise HTTPException(
                status_code=422,
                detail="No se pudo extraer texto con OCR avanzado.",
            )
        return {
            "extracted_text": extracted_text,
            "entities": extract_grouped_entities(extracted_text, incluir_genericas),
            "recomendaciones": recommendations,
            "motorOCRUsado": "paddleocr",
            "detalleOCR": {
                "cantidadCaracteres": detail["cantidadCaracteres"],
                "cantidadLineas": detail["cantidadLineas"],
                "confianzaPromedio": detail["confianzaPromedio"],
            },
        }
    except HTTPException:
        raise
    except Exception:
        logger.exception("Error inesperado en /SubirArchivoPaddle.")
        raise HTTPException(
            status_code=500,
            detail="Error interno al procesar el archivo con OCR avanzado.",
        )


@router.post(
    "/ExtraerEntidades",
    tags=["ProcesamientoArchivos"],
    dependencies=[Depends(require_internal_token("process", "ExtraerEntidades"))],
)
async def extract_document_entities(
    file: UploadFile = File(...),
    incluir_genericas: bool = Query(True, alias="incluirGenericas"),
):
    try:
        file_content, _ = await read_validated_upload(file, {DOCX_TYPE, DOC_TYPE})
        document_text = extract_text_from_document(file_content).strip()
        entities = extract_entities(document_text, incluir_genericas)
        return {
            "nombreArchivo": file.filename,
            "cantidadEntidades": len(entities),
            "entities": entities,
        }
    except HTTPException:
        raise
    except Exception:
        logger.exception("Error inesperado en /ExtraerEntidades.")
        raise HTTPException(status_code=500, detail="Error interno al extraer entidades.")
