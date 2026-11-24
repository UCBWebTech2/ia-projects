from contextlib import asynccontextmanager
import warnings

warnings.filterwarnings(
    "ignore",
    message='Field "model_kwargs" in TransformersEngineConfig has conflict with protected namespace "model_".*',
    category=UserWarning,
)

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from OCR_API.app.api.routers import admin, classification, ocr, processing, training
from OCR_API.app.core.config import settings
from OCR_API.app.services.document_service import configure_tesseract
from OCR_API.app.services.model_service import civil_penal_model_registry


@asynccontextmanager
async def lifespan(_: FastAPI):
    configure_tesseract()
    civil_penal_model_registry.load()
    yield


def create_app() -> FastAPI:
    application = FastAPI(lifespan=lifespan)
    application.add_middleware(
        CORSMiddleware,
        allow_origins=list(settings.cors_origins),
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )

    @application.get("/health")
    async def health():
        return {"status": "ok"}

    application.include_router(ocr.router)
    application.include_router(classification.router)
    application.include_router(processing.router)
    application.include_router(training.router)
    application.include_router(admin.router)
    return application


app = create_app()
