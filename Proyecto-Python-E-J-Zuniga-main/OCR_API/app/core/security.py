import base64
import json
import logging
import time
from dataclasses import dataclass
from threading import Lock
from typing import Any, Callable

from cryptography.exceptions import InvalidSignature
from cryptography.hazmat.primitives import hashes, serialization
from cryptography.hazmat.primitives.asymmetric import padding, rsa
from fastapi import Depends, HTTPException, status
from fastapi.security import HTTPAuthorizationCredentials, HTTPBearer

from OCR_API.app.core.config import settings

logger = logging.getLogger("uvicorn.error")


bearer_scheme = HTTPBearer(
    auto_error=False,
    scheme_name="InternalJWT",
    description="JWT interno RS256 generado por la API .NET 8.",
)


@dataclass(frozen=True)
class InternalPrincipal:
    subject: str
    actor_user_id: str
    permissions: frozenset[str]
    operation: str
    token_id: str


class ReplayCache:
    def __init__(self) -> None:
        self._used_token_ids: dict[str, int] = {}
        self._lock = Lock()

    def consume(self, token_id: str, expires_at: int, now: int) -> bool:
        with self._lock:
            self._used_token_ids = {
                used_id: expiration
                for used_id, expiration in self._used_token_ids.items()
                if expiration >= now
            }
            if token_id in self._used_token_ids:
                return False
            self._used_token_ids[token_id] = expires_at
            return True


replay_cache = ReplayCache()


def _unauthorized(detail: str = "Token interno inválido.") -> HTTPException:
    return HTTPException(
        status_code=status.HTTP_401_UNAUTHORIZED,
        detail=detail,
        headers={"WWW-Authenticate": "Bearer"},
    )


def _decode_base64url(value: str) -> bytes:
    try:
        padding_length = (-len(value)) % 4
        return base64.urlsafe_b64decode(value + ("=" * padding_length))
    except Exception as exc:
        raise _unauthorized() from exc


def _decode_json_segment(value: str) -> dict[str, Any]:
    try:
        decoded = json.loads(_decode_base64url(value))
        if not isinstance(decoded, dict):
            raise ValueError
        return decoded
    except (json.JSONDecodeError, UnicodeDecodeError, ValueError) as exc:
        raise _unauthorized() from exc


def _load_public_key() -> rsa.RSAPublicKey:
    path = settings.internal_jwt_public_key_path
    if not path.is_file():
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail="La validación del JWT interno no está configurada.",
        )

    try:
        public_key = serialization.load_pem_public_key(path.read_bytes())
    except Exception as exc:
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail="La clave pública del JWT interno no es válida.",
        ) from exc

    if not isinstance(public_key, rsa.RSAPublicKey):
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail="La clave pública configurada no es RSA.",
        )
    return public_key


def _validate_audience(audience: Any) -> bool:
    if isinstance(audience, str):
        return audience == settings.internal_jwt_audience
    if isinstance(audience, list):
        return settings.internal_jwt_audience in audience
    return False


def _require_string(payload: dict[str, Any], claim: str) -> str:
    value = payload.get(claim)
    if not isinstance(value, str) or not value.strip():
        raise _unauthorized()
    return value


def _require_timestamp(payload: dict[str, Any], claim: str) -> int:
    value = payload.get(claim)
    if isinstance(value, bool) or not isinstance(value, int):
        raise _unauthorized()
    return value


def validate_internal_token(
    token: str,
    required_permission: str,
    required_operation: str,
) -> InternalPrincipal:
    parts = token.split(".")
    if len(parts) != 3:
        raise _unauthorized()

    encoded_header, encoded_payload, encoded_signature = parts
    header = _decode_json_segment(encoded_header)
    payload = _decode_json_segment(encoded_payload)

    if header.get("alg") != settings.internal_jwt_algorithm:
        raise _unauthorized()
    if settings.internal_jwt_algorithm != "RS256":
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail="El algoritmo JWT interno configurado no está soportado.",
        )

    signing_input = f"{encoded_header}.{encoded_payload}".encode("ascii")
    try:
        _load_public_key().verify(
            _decode_base64url(encoded_signature),
            signing_input,
            padding.PKCS1v15(),
            hashes.SHA256(),
        )
    except (InvalidSignature, ValueError) as exc:
        raise _unauthorized() from exc

    now = int(time.time())
    skew = settings.internal_jwt_clock_skew_seconds
    issued_at = _require_timestamp(payload, "iat")
    expires_at = _require_timestamp(payload, "exp")

    if expires_at <= now - skew or issued_at > now + skew:
        raise _unauthorized()
    if expires_at <= issued_at:
        raise _unauthorized()
    if expires_at - issued_at > settings.internal_jwt_max_lifetime_seconds:
        raise _unauthorized()
    if payload.get("iss") != settings.internal_jwt_issuer:
        raise _unauthorized()
    if not _validate_audience(payload.get("aud")):
        raise _unauthorized()

    subject = _require_string(payload, "sub")
    if subject != settings.internal_jwt_subject:
        raise _unauthorized()

    actor_user_id = _require_string(payload, "actor_user_id")
    operation = _require_string(payload, "operation")
    token_id = _require_string(payload, "jti")

    permissions = payload.get("permissions")
    if (
        not isinstance(permissions, list)
        or not permissions
        or any(not isinstance(permission, str) for permission in permissions)
    ):
        raise _unauthorized()

    permission_set = frozenset(permissions)
    if required_permission not in permission_set or operation != required_operation:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="El token no autoriza esta operación.",
        )

    if not replay_cache.consume(token_id, expires_at, now):
        raise _unauthorized("El token interno ya fue utilizado.")

    logger.info(
        "JWT interno autorizado. actor_user_id=%s operation=%s jti=%s",
        actor_user_id,
        operation,
        token_id,
    )

    return InternalPrincipal(
        subject=subject,
        actor_user_id=actor_user_id,
        permissions=permission_set,
        operation=operation,
        token_id=token_id,
    )


def require_internal_token(
    permission: str,
    operation: str,
) -> Callable[..., InternalPrincipal]:
    def dependency(
        credentials: HTTPAuthorizationCredentials | None = Depends(bearer_scheme),
    ) -> InternalPrincipal:
        if credentials is None or credentials.scheme.lower() != "bearer":
            raise _unauthorized("Se requiere un token interno.")
        return validate_internal_token(
            credentials.credentials,
            required_permission=permission,
            required_operation=operation,
        )

    return dependency
