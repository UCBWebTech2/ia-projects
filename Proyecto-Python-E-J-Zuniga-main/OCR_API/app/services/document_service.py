import csv
import logging

import cv2
import numpy as np
import pytesseract
import tika
from fastapi import HTTPException
from tika import parser

from OCR_API.app.core.config import settings

logger = logging.getLogger("uvicorn.error")

tika.initVM()


def configure_tesseract() -> None:
    if settings.tesseract_cmd.exists():
        pytesseract.pytesseract.tesseract_cmd = str(settings.tesseract_cmd)
        logger.info("Ruta de Tesseract configurada correctamente.")
    else:
        logger.warning("No se encontró Tesseract: %s", settings.tesseract_cmd)


def convert_to_grayscale(image_bytes: bytes) -> np.ndarray:
    image = cv2.imdecode(np.frombuffer(image_bytes, np.uint8), cv2.IMREAD_COLOR)
    if image is None:
        raise ValueError("Error al leer la imagen.")
    return cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)


def extract_text_from_image(image_bytes: bytes) -> str:
    try:
        return pytesseract.image_to_string(
            convert_to_grayscale(image_bytes),
            lang="spa",
        )
    except Exception:
        logger.exception("Error inesperado al extraer texto desde imagen.")
        raise HTTPException(
            status_code=500,
            detail="Error interno al procesar la imagen.",
        )


def extract_text_from_document(file_content: bytes) -> str:
    try:
        parsed = parser.from_buffer(file_content)
        if not isinstance(parsed, dict):
            raise HTTPException(
                status_code=422,
                detail="No se pudo procesar correctamente el documento.",
            )

        document_text = parsed.get("content", "")
        if not document_text or not document_text.strip():
            raise HTTPException(
                status_code=422,
                detail="No se pudo extraer texto útil del documento.",
            )
        return document_text.strip()
    except HTTPException:
        raise
    except Exception:
        logger.exception("Error inesperado al extraer texto desde documento.")
        raise HTTPException(
            status_code=500,
            detail="Error interno al procesar el documento.",
        )


def validate_image_conditions(image_bytes: bytes) -> list[str]:
    errors: list[str] = []
    image = cv2.imdecode(np.frombuffer(image_bytes, np.uint8), cv2.IMREAD_COLOR)
    if image is None:
        return ["No se pudo decodificar la imagen."]

    _, width = image.shape[:2]
    if width < 1000:
        errors.append(
            "Resolución insuficiente: la imagen debe superar los 1000 píxeles de ancho."
        )

    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
    if float(np.mean(gray)) < 80:
        errors.append("Imagen oscura: asegúrese de tener buena iluminación.")
    if float(cv2.Laplacian(gray, cv2.CV_64F).var()) < 100:
        errors.append("La imagen está desenfocada: asegúrese de que sea nítida.")
    return errors


def _normalize_dataset_text(text: str) -> str:
    return " ".join(str(text).split()).strip()


def append_dataset_rows(rows: list[tuple[str, str]]) -> dict[str, int]:
    """Append validated dataset rows safely.

    Improvements (Phase 0): anchor to settings path, validate schema/values,
    dedupe by normalized text hash, backup before write, atomic replace.
    """
    import hashlib
    import os
    import shutil
    import tempfile

    allowed_labels = {"civil", "penal"}
    dataset_path = settings.dataset_path
    dataset_path.parent.mkdir(parents=True, exist_ok=True)

    existing_hashes: set[str] = set()
    valid_rows: list[tuple[str, str]] = []
    invalid_rows = 0
    duplicate_rows = 0

    if dataset_path.is_file():
        with dataset_path.open("r", newline="", encoding="utf-8") as csv_file:
            reader = csv.DictReader(csv_file)
            for row in reader:
                text = _normalize_dataset_text(row.get("texto", ""))
                label = _normalize_dataset_text(row.get("etiqueta", "")).lower()
                if not text or label not in allowed_labels:
                    continue
                existing_hashes.add(hashlib.sha256(f"{label}\n{text}".encode("utf-8")).hexdigest())

    for raw_text, raw_label in rows:
        text = _normalize_dataset_text(raw_text)
        label = _normalize_dataset_text(raw_label).lower()
        if not text or label not in allowed_labels:
            invalid_rows += 1
            continue
        row_hash = hashlib.sha256(f"{label}\n{text}".encode("utf-8")).hexdigest()
        if row_hash in existing_hashes:
            duplicate_rows += 1
            continue
        existing_hashes.add(row_hash)
        valid_rows.append((text, label))

    if not valid_rows:
        logger.info(
            "No se agregaron filas al dataset. invalidas=%s duplicadas=%s",
            invalid_rows,
            duplicate_rows,
        )
        return {"added": 0, "invalid": invalid_rows, "duplicates": duplicate_rows}

    # Backup before mutating an existing dataset.
    if dataset_path.is_file():
        backup_path = dataset_path.with_suffix(dataset_path.suffix + ".bak")
        shutil.copy2(dataset_path, backup_path)

    fd, tmp_name = tempfile.mkstemp(dir=str(dataset_path.parent), suffix=".tmp")
    try:
        with os.fdopen(fd, "w", newline="", encoding="utf-8") as csv_file:
            writer = csv.writer(csv_file)
            writer.writerow(["texto", "etiqueta"])
            # Preserve existing valid rows.
            if dataset_path.is_file():
                with dataset_path.open("r", newline="", encoding="utf-8") as existing_file:
                    reader = csv.DictReader(existing_file)
                    for row in reader:
                        text = _normalize_dataset_text(row.get("texto", ""))
                        label = _normalize_dataset_text(row.get("etiqueta", "")).lower()
                        if text and label in allowed_labels:
                            writer.writerow([text, label])
            writer.writerows(valid_rows)
        os.replace(tmp_name, dataset_path)
    except Exception:
        try:
            os.remove(tmp_name)
        except OSError:
            pass
        raise

    logger.info(
        "Dataset actualizado. agregadas=%s invalidas=%s duplicadas=%s ruta=%s",
        len(valid_rows),
        invalid_rows,
        duplicate_rows,
        dataset_path,
    )
    return {"added": len(valid_rows), "invalid": invalid_rows, "duplicates": duplicate_rows}
