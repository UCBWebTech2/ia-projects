import logging
import re
from threading import Lock
from typing import Any

import joblib

from OCR_API.app.core.config import settings
from OCR_API.scripts.EntityRuler import nlp

logger = logging.getLogger("uvicorn.error")


class CivilPenalModelRegistry:
    def __init__(self) -> None:
        self.model: Any = None
        self._lock = Lock()

    def load(self) -> bool:
        with self._lock:
            if not settings.civil_penal_model_path.exists():
                logger.error(
                    "Archivo del modelo Civil/Penal no encontrado: %s",
                    settings.civil_penal_model_path.name,
                )
                self.model = None
                return False

            try:
                self.model = joblib.load(settings.civil_penal_model_path)
            except Exception:
                logger.exception("Error inesperado al cargar el modelo Civil/Penal.")
                self.model = None
                return False

            logger.info("Modelo Civil/Penal cargado correctamente.")
            return True

    @property
    def available(self) -> bool:
        return self.model is not None

    def predict(self, text: str) -> dict[str, object]:
        prediction = str(self.model.predict([text])[0]).lower()
        probabilities = self.model.predict_proba([text])[0]
        classes = [str(class_name).lower() for class_name in self.model.classes_]
        probability_by_class = {
            class_name: float(probability)
            for class_name, probability in zip(classes, probabilities)
        }
        confidence = float(max(probabilities)) if len(probabilities) else 0.0

        # Generic contract (Phase 2) + legacy compatibility fields.
        response: dict[str, object] = {
            "categoria": prediction,
            "confianza": confidence,
            "probabilidades": probability_by_class,
            "version_modelo": settings.civil_penal_model_path.name,
        }
        # Legacy binary keys kept for backward compatibility.
        response["probabilidadCivil"] = probability_by_class.get("civil", 0.0)
        response["probabilidadPenal"] = probability_by_class.get("penal", 0.0)
        return response


civil_penal_model_registry = CivilPenalModelRegistry()


def normalizar_texto(texto: str) -> str:
    if not texto:
        return ""
    return re.sub(r"\s+", " ", texto).strip()


def preprocess_text(text: str) -> str:
    """Lematiza y limpia texto para entrenamiento/ingesta de dataset."""
    normalized = (text or "").lower()
    normalized = re.sub(r"[^a-záéíóúüñ\s]", "", normalized)
    doc = nlp(normalized)
    return " ".join(
        token.lemma_
        for token in doc
        if not token.is_stop and not token.is_punct
    )


def clasificar_documento(texto: str) -> dict[str, float | str]:
    texto_normalizado = normalizar_texto(texto)
    return civil_penal_model_registry.predict(texto_normalizado)
