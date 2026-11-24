import logging
import time

from fastapi import APIRouter, Depends, File, HTTPException, UploadFile

from OCR_API.app.api.upload_validation import IMAGE_TYPES, read_validated_upload
from OCR_API.app.core.security import require_internal_token
from OCR_API.app.services.document_service import convert_to_grayscale, extract_text_from_image
from OCR_API.app.services.entity_service import extract_grouped_entities
from OCR_API.app.services.ocr_service import (
    compare_tesseract_methods,
    extraer_texto_paddle_desde_imagen,
    extract_text_from_image_auto_detalle,
)
from OCR_API.scripts.EntityRuler import eliminar_duplicados_semanticos, find_entities

router = APIRouter()
logger = logging.getLogger("uvicorn.error")


@router.post(
    "/ProbarPaddleOCRImagen",
    tags=["OCR"],
    dependencies=[Depends(require_internal_token("admin", "ProbarPaddleOCRImagen"))],
)
async def test_paddle_ocr_image(file: UploadFile = File(...)):
    try:
        image_bytes, content_type = await read_validated_upload(file, IMAGE_TYPES)
        start = time.perf_counter()
        detail = extraer_texto_paddle_desde_imagen(image_bytes)
        elapsed = round(time.perf_counter() - start, 3)
        entities = extract_grouped_entities(detail["texto"])
        return {
            "nombreArchivo": file.filename,
            "tipoContenido": content_type,
            "motorOCR": "PaddleOCR",
            "tiempoSegundos": elapsed,
            "cantidadCaracteres": detail["cantidadCaracteres"],
            "cantidadLineas": detail["cantidadLineas"],
            "confianzaPromedio": detail["confianzaPromedio"],
            "texto": detail["texto"],
            "entities": entities,
            "cantidadEntidadesUnicas": len(entities),
            "lineasPreview": detail["lineas"][:80],
        }
    except HTTPException:
        raise
    except Exception:
        logger.exception("Error inesperado probando PaddleOCR.")
        raise HTTPException(status_code=500, detail="Error interno al probar PaddleOCR.")


@router.post(
    "/ProbarOCRAutoImagen",
    tags=["OCR"],
    dependencies=[Depends(require_internal_token("admin", "ProbarOCRAutoImagen"))],
)
async def test_auto_ocr_image(file: UploadFile = File(...)):
    try:
        image_bytes, content_type = await read_validated_upload(file, IMAGE_TYPES)
        detail = extract_text_from_image_auto_detalle(image_bytes)
        return {
            "nombreArchivo": file.filename,
            "tipoContenido": content_type,
            "metodoPrincipal": detail["metodoPrincipal"],
            "estrategiaFinal": detail.get(
                "estrategiaFinal",
                "selector_automatico_con_combinacion_por_lineas",
            ),
            "scorePrincipal": detail["scorePrincipal"],
            "scoreTextoCombinado": detail["scoreTextoCombinado"],
            "cantidadCaracteres": detail["cantidadCaracteres"],
            "cantidadLineasUsadas": detail["cantidadLineasUsadas"],
            "texto": detail["texto"],
            "lineasUsadasPreview": detail["lineasUsadas"][:80],
            "resultadosBase": detail["resultadosBase"],
        }
    except HTTPException:
        raise
    except Exception:
        logger.exception("Error inesperado probando OCR automático.")
        raise HTTPException(status_code=500, detail="Error interno al probar OCR automático.")


@router.post(
    "/CompararOCR",
    tags=["OCR"],
    dependencies=[Depends(require_internal_token("admin", "CompararOCR"))],
)
async def compare_ocr(file: UploadFile = File(...)):
    try:
        image_bytes, _ = await read_validated_upload(file, IMAGE_TYPES)

        start = time.perf_counter()
        normal_text = extract_text_from_image(image_bytes)
        normal_time = round(time.perf_counter() - start, 3)

        start = time.perf_counter()
        auto_detail = extract_text_from_image_auto_detalle(image_bytes)
        auto_time = round(time.perf_counter() - start, 3)
        auto_text = auto_detail["texto"]

        normal_entities = find_entities(normal_text, eliminar_repetidos=False)
        auto_entities = find_entities(auto_text, eliminar_repetidos=False)
        unique_normal = eliminar_duplicados_semanticos(normal_entities, incluir_frecuencia=True)
        unique_auto = eliminar_duplicados_semanticos(auto_entities, incluir_frecuencia=True)
        return {
            "archivo": file.filename,
            "ocrNormal": {
                "tiempoSegundos": normal_time,
                "cantidadCaracteres": len(normal_text),
                "cantidadEntidades": len(normal_entities),
                "cantidadEntidadesUnicas": len(unique_normal),
                "textoPreview": normal_text[:1000],
            },
            "ocrAuto": {
                "tiempoSegundos": auto_time,
                "cantidadCaracteres": len(auto_text),
                "cantidadEntidades": len(auto_entities),
                "cantidadEntidadesUnicas": len(unique_auto),
                "metodoPrincipal": auto_detail["metodoPrincipal"],
                "scorePrincipal": auto_detail["scorePrincipal"],
                "scoreTextoCombinado": auto_detail["scoreTextoCombinado"],
                "cantidadLineasUsadas": auto_detail["cantidadLineasUsadas"],
                "textoPreview": auto_text[:1000],
            },
        }
    except HTTPException:
        raise
    except Exception:
        logger.exception("Error comparando OCR.")
        raise HTTPException(status_code=500, detail="Error interno al comparar OCR.")


@router.post(
    "/CompararOCRImagen",
    tags=["Pruebas OCR"],
    dependencies=[Depends(require_internal_token("admin", "CompararOCRImagen"))],
)
async def compare_ocr_image(file: UploadFile = File(...)):
    try:
        image_bytes, content_type = await read_validated_upload(file, IMAGE_TYPES)
        results = compare_tesseract_methods(
            convert_to_grayscale(image_bytes),
            image_bytes,
        )
        return {
            "nombreArchivo": file.filename,
            "tipoContenido": content_type,
            "resultados": results,
        }
    except HTTPException:
        raise
    except Exception:
        logger.exception("Error al comparar métodos OCR.")
        raise HTTPException(status_code=500, detail="Error interno al comparar métodos OCR.")
