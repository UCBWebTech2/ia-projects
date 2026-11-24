import io
import zipfile

import cv2
import numpy as np
from fastapi import HTTPException, UploadFile

from OCR_API.app.core.config import settings


IMAGE_TYPES = {"image/png", "image/jpeg", "image/jpg"}
PDF_TYPE = "application/pdf"
DOCX_TYPE = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
DOC_TYPE = "application/msword"
DOCUMENT_TYPES = {PDF_TYPE, DOCX_TYPE, DOC_TYPE}
SUPPORTED_TYPES = IMAGE_TYPES | DOCUMENT_TYPES

PNG_SIGNATURE = b"\x89PNG\r\n\x1a\n"
JPEG_SIGNATURE = b"\xff\xd8\xff"
PDF_SIGNATURE = b"%PDF-"
OLE_SIGNATURE = b"\xd0\xcf\x11\xe0\xa1\xb1\x1a\xe1"


def _matches_declared_type(content: bytes, content_type: str) -> bool:
    if content_type == "image/png":
        return content.startswith(PNG_SIGNATURE)
    if content_type in {"image/jpeg", "image/jpg"}:
        return content.startswith(JPEG_SIGNATURE)
    if content_type == PDF_TYPE:
        return content.startswith(PDF_SIGNATURE)
    if content_type == DOC_TYPE:
        return content.startswith(OLE_SIGNATURE)
    if content_type == DOCX_TYPE:
        try:
            with zipfile.ZipFile(io.BytesIO(content)) as archive:
                names = set(archive.namelist())
                return (
                    "[Content_Types].xml" in names
                    and "word/document.xml" in names
                )
        except (OSError, zipfile.BadZipFile):
            return False
    return False


def _validate_image(content: bytes) -> None:
    image = cv2.imdecode(np.frombuffer(content, np.uint8), cv2.IMREAD_COLOR)
    if image is None:
        raise HTTPException(
            status_code=400,
            detail="El contenido recibido no es una imagen válida.",
        )

    height, width = image.shape[:2]
    if height * width > settings.max_image_pixels:
        raise HTTPException(
            status_code=413,
            detail=(
                "La imagen supera el límite permitido de "
                f"{settings.max_image_pixels} píxeles."
            ),
        )


async def read_validated_upload(
    file: UploadFile,
    allowed_types: set[str],
) -> tuple[bytes, str]:
    if not file.content_type:
        raise HTTPException(
            status_code=400,
            detail="No se pudo determinar el tipo de archivo.",
        )

    content_type = file.content_type.lower()
    if content_type not in allowed_types:
        raise HTTPException(status_code=400, detail="Tipo de archivo no válido.")

    max_size = (
        settings.max_image_size_bytes
        if content_type in IMAGE_TYPES
        else settings.max_document_size_bytes
    )
    content_length = file.headers.get("content-length")
    if content_length:
        try:
            if int(content_length) > max_size:
                raise HTTPException(
                    status_code=413,
                    detail=(
                        "El archivo supera el límite permitido de "
                        f"{max_size // (1024 * 1024)} MB."
                    ),
                )
        except ValueError:
            raise HTTPException(
                status_code=400,
                detail="La longitud declarada del archivo no es válida.",
            )

    content = bytearray()

    while chunk := await file.read(settings.upload_chunk_size):
        content.extend(chunk)
        if len(content) > max_size:
            raise HTTPException(
                status_code=413,
                detail=f"El archivo supera el límite permitido de {max_size // (1024 * 1024)} MB.",
            )

    if not content:
        raise HTTPException(status_code=400, detail="El archivo está vacío.")

    file_content = bytes(content)
    if not _matches_declared_type(file_content, content_type):
        raise HTTPException(
            status_code=400,
            detail="El contenido real del archivo no coincide con el tipo declarado.",
        )

    if content_type in IMAGE_TYPES:
        _validate_image(file_content)

    return file_content, content_type
