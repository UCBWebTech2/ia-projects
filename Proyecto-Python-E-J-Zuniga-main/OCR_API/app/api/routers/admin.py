import json
import logging
from pathlib import Path

from fastapi import APIRouter, Depends, HTTPException

from OCR_API.app.core.config import settings
from OCR_API.app.core.security import require_internal_token
from OCR_API.app.services.model_service import civil_penal_model_registry

router = APIRouter()
logger = logging.getLogger("uvicorn.error")


@router.post(
    "/admin/recargar-modelo",
    tags=["Entrenamiento"],
    dependencies=[Depends(require_internal_token("admin", "RecargarModelo"))],
)
def reload_model():
    try:
        civil_penal_loaded = civil_penal_model_registry.load()
        if not civil_penal_loaded:
            raise HTTPException(
                status_code=500,
                detail="No se pudo recargar el modelo Civil/Penal.",
            )
        return {
            "status": "ok",
            "mensaje": "Modelo Civil/Penal recargado correctamente.",
        }
    except HTTPException:
        raise
    except Exception:
        logger.exception("Error inesperado en /admin/recargar-modelo.")
        raise HTTPException(status_code=500, detail="Error interno al recargar el modelo.")


@router.get(
    "/admin/model-metrics",
    tags=["Entrenamiento"],
    dependencies=[Depends(require_internal_token("admin", "ModelMetrics"))],
)
def model_metrics():
    try:
        model_path = settings.civil_penal_model_path
        metadata_path = model_path.with_suffix(".metadata.json")
        payload = {
            "model_path": str(model_path),
            "metadata_path": str(metadata_path),
            "available": bool(civil_penal_model_registry.available),
            "metadata_found": metadata_path.exists(),
            "metrics": None,
            "metadata": None,
        }
        if metadata_path.exists():
            metadata = json.loads(metadata_path.read_text(encoding="utf-8"))
            payload["metadata"] = metadata
            payload["metrics"] = metadata.get("metrics")
            payload["pipeline"] = metadata.get("pipeline")
            payload["datasets"] = metadata.get("datasets")
            payload["created_at_utc"] = metadata.get("created_at_utc")
            payload["acceptance_gate"] = metadata.get("acceptance_gate")
        else:
            # Fallback: expose minimal live info
            model = civil_penal_model_registry.model
            if model is not None:
                classes = [str(c) for c in getattr(model, "classes_", [])]
                payload["metrics"] = {
                    "accuracy": None,
                    "macro_f1": None,
                    "weighted_f1": None,
                    "labels": classes,
                    "confusion_matrix": None,
                    "classification_report": None,
                }
        return payload
    except Exception:
        logger.exception("Error inesperado en /admin/model-metrics.")
        raise HTTPException(status_code=500, detail="Error interno al obtener métricas del modelo.")