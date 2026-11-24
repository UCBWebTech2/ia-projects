import logging

from fastapi import APIRouter, Depends, File, HTTPException, UploadFile

from OCR_API.app.api.upload_validation import IMAGE_TYPES, SUPPORTED_TYPES, read_validated_upload
from OCR_API.app.core.security import require_internal_token
from OCR_API.app.services.document_service import extract_text_from_document, extract_text_from_image
from OCR_API.app.services.model_service import (
    civil_penal_model_registry,
    clasificar_documento,
    normalizar_texto,
)
from OCR_API.app.services.ocr_service import evaluar_texto_ocr, extract_text_from_image_auto_detalle

router = APIRouter()
logger = logging.getLogger("uvicorn.error")


def _extract_text_for_classification(
    file_content: bytes,
    content_type: str,
) -> tuple[str, str | None]:
    if content_type not in IMAGE_TYPES:
        return extract_text_from_document(file_content), "document_parser"

    normal_text = (extract_text_from_image(file_content) or "").strip()
    normal_score = evaluar_texto_ocr(normal_text)
    if normal_text and normal_score >= 450 and len(normal_text) >= 800:
        return normal_text, "tesseract_normal"

    auto_detail = extract_text_from_image_auto_detalle(file_content)
    auto_text = (auto_detail["texto"] or "").strip()
    if auto_text and auto_detail["scoreTextoCombinado"] >= normal_score:
        return auto_text, "tesseract_auto"
    return normal_text, "tesseract_normal"


@router.post(
    "/ProbarClasificador",
    tags=["Clasificación"],
    dependencies=[Depends(require_internal_token("admin", "ProbarClasificador"))],
)
async def test_classifier(file: UploadFile = File(...)):
    try:
        if not civil_penal_model_registry.available:
            raise HTTPException(
                status_code=500,
                detail="El modelo Civil/Penal no está disponible.",
            )

        file_content, content_type = await read_validated_upload(file, SUPPORTED_TYPES)
        extracted_text, _ = _extract_text_for_classification(file_content, content_type)
        normalized_text = normalizar_texto(extracted_text)

        if not normalized_text:
            raise HTTPException(status_code=422, detail="No se pudo extraer texto útil del archivo.")

        result = clasificar_documento(normalized_text)
        return _format_civil_penal_response(result, None, len(normalized_text))
    except HTTPException:
        raise
    except Exception:
        logger.exception("Error inesperado en /ProbarClasificador.")
        raise HTTPException(status_code=500, detail="Error interno al probar el clasificador.")


def _format_civil_penal_response(
    result: dict[str, object],
    motor_ocr_usado: str | None,
    cantidad_caracteres: int,
) -> dict[str, object]:
    probabilidades = result.get("probabilidades", {}) or {}
    if not isinstance(probabilidades, dict):
        probabilidades = {}

    response: dict[str, object] = {
        "categoria": result.get("categoria"),
        "confianza": round(float(result.get("confianza", 0.0)), 4),
        "probabilidades": {str(k): round(float(v), 4) for k, v in probabilidades.items()},
        "versionModelo": result.get("version_modelo"),
        "motorOCRUsado": motor_ocr_usado,
        "cantidadCaracteres": cantidad_caracteres,
    }
    # Legacy binary fields for backward compatibility.
    response["probabilidadCivil"] = round(float(result.get("probabilidadCivil", probabilidades.get("civil", 0.0))), 4)
    response["probabilidadPenal"] = round(float(result.get("probabilidadPenal", probabilidades.get("penal", 0.0))), 4)
    return response


@router.post(
    "/SubirArchivoClasificado",
    tags=["Clasificación"],
    dependencies=[Depends(require_internal_token("process", "SubirArchivoClasificado"))],
)
async def upload_classified_file(file: UploadFile = File(...)):
    try:
        if not civil_penal_model_registry.available:
            raise HTTPException(
                status_code=500,
                detail="El modelo Civil/Penal no está disponible.",
            )

        file_content, content_type = await read_validated_upload(file, SUPPORTED_TYPES)
        extracted_text, ocr_engine = _extract_text_for_classification(file_content, content_type)
        normalized_text = normalizar_texto(extracted_text)

        if not normalized_text:
            raise HTTPException(status_code=422, detail="No se pudo extraer texto útil del archivo.")

        result = clasificar_documento(normalized_text)
        return _format_civil_penal_response(
            result,
            ocr_engine,
            len(normalized_text),
        )
    except HTTPException:
        raise
    except Exception:
        logger.exception("Error inesperado en /SubirArchivoClasificado.")
        raise HTTPException(status_code=500, detail="Error interno al clasificar el archivo.")
