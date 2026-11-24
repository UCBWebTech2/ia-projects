import logging
import tempfile
import os
from typing import Any

from paddleocr import PaddleOCR

logger = logging.getLogger("uvicorn.error")

paddle_ocr = None


def cargar_paddle_ocr():
    global paddle_ocr

    if paddle_ocr is not None:
        return paddle_ocr

    logger.info("Inicializando PaddleOCR.")

    paddle_ocr = PaddleOCR(
        lang="es"
    )
    logger.info("PaddleOCR inicializado correctamente.")

    return paddle_ocr


def extraer_texto_paddle_desde_imagen(image_bytes: bytes) -> dict[str, Any]:
    ocr = cargar_paddle_ocr()

    with tempfile.NamedTemporaryFile(delete=False, suffix=".jpg") as temp_file:
        temp_file.write(image_bytes)
        temp_path = temp_file.name

    try:
        resultado = ocr.predict(temp_path)

        lineas = []

        for res in resultado:
            # PaddleOCR 3.x devuelve OCRResult, que se comporta como diccionario
            textos = res.get("rec_texts", [])
            scores = res.get("rec_scores", [])
            polys = res.get("rec_polys", []) or res.get("dt_polys", [])

            for texto, score, poly in zip(textos, scores, polys):
                if not texto or not str(texto).strip():
                    continue

                confianza = float(score)

                if confianza < 0.60:
                    continue

                xs = [int(punto[0]) for punto in poly]
                ys = [int(punto[1]) for punto in poly]

                lineas.append({
                    "texto": str(texto).strip(),
                    "confianza": round(confianza, 4),
                    "left": min(xs),
                    "top": min(ys),
                    "right": max(xs),
                    "bottom": max(ys)
                })

        lineas = sorted(
            lineas,
            key=lambda item: (item["top"], item["left"])
        )

        texto_final = "\n".join(
            item["texto"]
            for item in lineas
            if item["texto"].strip()
        )

        confianza_promedio = (
            sum(item["confianza"] for item in lineas) / len(lineas)
            if lineas
            else 0
        )

        return {
            "texto": texto_final.strip(),
            "cantidadCaracteres": len(texto_final.strip()),
            "cantidadLineas": len(lineas),
            "confianzaPromedio": round(confianza_promedio, 4),
            "lineas": lineas
        }

    finally:
        try:
            os.remove(temp_path)
        except Exception:
            logger.warning("No se pudo eliminar archivo temporal de PaddleOCR.")