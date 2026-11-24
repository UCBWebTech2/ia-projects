import cv2
import numpy as np
import pytesseract

from OCR_API.scripts.OCR import (
    evaluar_texto_ocr,
    extract_text_from_image_auto_detalle,
)
from OCR_API.scripts.OCRPaddle import extraer_texto_paddle_desde_imagen


def preprocess_otsu(image_bytes: bytes) -> np.ndarray:
    image = cv2.imdecode(np.frombuffer(image_bytes, np.uint8), cv2.IMREAD_COLOR)
    if image is None:
        raise ValueError("No se pudo decodificar la imagen para OCR.")
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
    gray = cv2.bilateralFilter(gray, 11, 17, 17)
    _, threshold = cv2.threshold(
        gray,
        0,
        255,
        cv2.THRESH_BINARY + cv2.THRESH_OTSU,
    )
    return threshold


def preprocess_adaptive(image_bytes: bytes) -> np.ndarray:
    image = cv2.imdecode(np.frombuffer(image_bytes, np.uint8), cv2.IMREAD_COLOR)
    if image is None:
        raise ValueError("No se pudo decodificar la imagen para OCR.")
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
    gray = cv2.bilateralFilter(gray, 11, 17, 17)
    threshold = cv2.adaptiveThreshold(
        gray,
        255,
        cv2.ADAPTIVE_THRESH_GAUSSIAN_C,
        cv2.THRESH_BINARY,
        11,
        2,
    )
    coordinates = np.column_stack(np.where(threshold > 0))
    if coordinates.size == 0:
        return threshold
    angle = cv2.minAreaRect(coordinates)[-1]
    angle = -(90 + angle) if angle < -45 else -angle
    height, width = threshold.shape
    matrix = cv2.getRotationMatrix2D((width // 2, height // 2), angle, 1.0)
    return cv2.warpAffine(
        threshold,
        matrix,
        (width, height),
        flags=cv2.INTER_CUBIC,
        borderMode=cv2.BORDER_REPLICATE,
    )


def compare_tesseract_methods(gray_image: np.ndarray, image_bytes: bytes) -> dict:
    configurations = {
        "gray_psm3": ("Escala de grises con --oem 3 --psm 3.", gray_image, "--oem 3 --psm 3"),
        "gray_psm4": ("Escala de grises con --oem 3 --psm 4.", gray_image, "--oem 3 --psm 4"),
        "gray_psm11": ("Escala de grises con --oem 3 --psm 11.", gray_image, "--oem 3 --psm 11"),
        "gray_default_actual": ("Escala de grises, sin config. Método actual.", gray_image, ""),
        "gray_psm6": ("Escala de grises con --oem 3 --psm 6.", gray_image, "--oem 3 --psm 6"),
        "otsu_psm6": ("Bilateral filter + Otsu + --oem 3 --psm 6.", preprocess_otsu(image_bytes), "--oem 3 --psm 6"),
        "adaptive_deskew_psm6": ("Umbral adaptativo + corrección de inclinación + --oem 3 --psm 6.", preprocess_adaptive(image_bytes), "--oem 3 --psm 6"),
    }
    results = {}
    for name, (description, image, config) in configurations.items():
        text = pytesseract.image_to_string(image, lang="spa", config=config).strip()
        results[name] = {
            "descripcion": description,
            "cantidad_caracteres": len(text),
            "texto": text,
        }
    return results


__all__ = [
    "evaluar_texto_ocr",
    "extract_text_from_image_auto_detalle",
    "extraer_texto_paddle_desde_imagen",
]
