import os
import platform
import shutil
from dataclasses import dataclass
from pathlib import Path


def _get_int(name: str, default: int) -> int:
    return int(os.getenv(name, default))


def _get_path(name: str, default: Path, base_dir: Path) -> Path:
    path = Path(os.getenv(name, str(default))).expanduser()
    if not path.is_absolute():
        path = base_dir / path
    return path.resolve()


def _load_env_file(path: Path) -> None:
    if not path.is_file():
        return

    for raw_line in path.read_text(encoding="utf-8").splitlines():
        line = raw_line.strip()
        if not line or line.startswith("#") or "=" not in line:
            continue
        key, value = line.split("=", 1)
        os.environ.setdefault(key.strip(), value.strip().strip("\"'"))


@dataclass(frozen=True)
class Settings:
    project_root: Path
    ocr_api_root: Path
    files_dir: Path
    models_dir: Path
    scripts_dir: Path
    model_path: Path
    vectorizer_path: Path
    civil_penal_model_path: Path
    dataset_path: Path
    tesseract_cmd: Path
    cors_origins: tuple[str, ...]
    max_image_size_bytes: int
    max_document_size_bytes: int
    max_image_pixels: int
    upload_chunk_size: int
    internal_jwt_public_key_path: Path
    internal_jwt_issuer: str
    internal_jwt_audience: str
    internal_jwt_subject: str
    internal_jwt_algorithm: str
    internal_jwt_max_lifetime_seconds: int
    internal_jwt_clock_skew_seconds: int


def load_settings() -> Settings:
    ocr_api_root = Path(__file__).resolve().parents[2]
    project_root = ocr_api_root.parent
    _load_env_file(project_root / ".env")
    files_dir = _get_path("FILES_DIR", ocr_api_root / "files", project_root)
    models_dir = _get_path("MODELS_DIR", ocr_api_root / "models", project_root)
    scripts_dir = _get_path("SCRIPTS_DIR", ocr_api_root / "scripts", project_root)
    cors_origins = tuple(
        origin.strip()
        for origin in os.getenv(
            "CORS_ORIGINS",
            "http://localhost:5159,http://localhost:5290",
        ).split(",")
        if origin.strip()
    )

    return Settings(
        project_root=project_root,
        ocr_api_root=ocr_api_root,
        files_dir=files_dir,
        models_dir=models_dir,
        scripts_dir=scripts_dir,
        model_path=_get_path(
            "MODEL_PATH",
            models_dir / "naive_bayes_model.pkl",
            project_root,
        ),
        vectorizer_path=_get_path(
            "VECTORIZER_PATH",
            models_dir / "vectorizer.pkl",
            project_root,
        ),
        civil_penal_model_path=_get_path(
            "CIVIL_PENAL_MODEL_PATH",
            models_dir / "modelo_civil_penal_v1.joblib",
            project_root,
        ),
        dataset_path=_get_path(
            "DATASET_PATH",
            files_dir / "dataset_derecho.csv",
            project_root,
        ),
        tesseract_cmd=_get_path(
            "TESSERACT_CMD",
            Path(
                shutil.which("tesseract")
                or (
                    r"C:\Program Files\Tesseract-OCR\tesseract.exe"
                    if platform.system() == "Windows"
                    else "/usr/bin/tesseract"
                )
            ),
            project_root,
        ),
        cors_origins=cors_origins,
        max_image_size_bytes=_get_int("MAX_IMAGE_SIZE_MB", 15) * 1024 * 1024,
        max_document_size_bytes=_get_int("MAX_DOCUMENT_SIZE_MB", 30) * 1024 * 1024,
        max_image_pixels=_get_int("MAX_IMAGE_PIXELS", 40_000_000),
        upload_chunk_size=_get_int("UPLOAD_CHUNK_SIZE_KB", 1024) * 1024,
        internal_jwt_public_key_path=_get_path(
            "INTERNAL_JWT_PUBLIC_KEY_PATH",
            project_root / "secrets" / "internal-jwt-public.pem",
            project_root,
        ),
        internal_jwt_issuer=os.getenv(
            "INTERNAL_JWT_ISSUER",
            "estudio-juridico-api",
        ),
        internal_jwt_audience=os.getenv("INTERNAL_JWT_AUDIENCE", "ocr-api"),
        internal_jwt_subject=os.getenv(
            "INTERNAL_JWT_SUBJECT",
            "estudio-juridico-api",
        ),
        internal_jwt_algorithm=os.getenv("INTERNAL_JWT_ALGORITHM", "RS256"),
        internal_jwt_max_lifetime_seconds=_get_int(
            "INTERNAL_JWT_MAX_LIFETIME_SECONDS",
            60,
        ),
        internal_jwt_clock_skew_seconds=_get_int(
            "INTERNAL_JWT_CLOCK_SKEW_SECONDS",
            5,
        ),
    )


settings = load_settings()
