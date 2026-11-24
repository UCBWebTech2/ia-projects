import base64
import json
import os
import tempfile
import time
import unittest
import uuid
from pathlib import Path

from cryptography.hazmat.primitives import hashes, serialization
from cryptography.hazmat.primitives.asymmetric import padding, rsa


TEMP_DIRECTORY = tempfile.TemporaryDirectory()
PRIVATE_KEY = rsa.generate_private_key(public_exponent=65537, key_size=2048)
PUBLIC_KEY_PATH = Path(TEMP_DIRECTORY.name) / "internal-jwt-public.pem"
PUBLIC_KEY_PATH.write_bytes(
    PRIVATE_KEY.public_key().public_bytes(
        encoding=serialization.Encoding.PEM,
        format=serialization.PublicFormat.SubjectPublicKeyInfo,
    )
)
os.environ["INTERNAL_JWT_PUBLIC_KEY_PATH"] = str(PUBLIC_KEY_PATH)

from fastapi.testclient import TestClient

from OCR_API.app.main import app


def encode_base64url(value: bytes) -> str:
    return base64.urlsafe_b64encode(value).rstrip(b"=").decode("ascii")


def create_token(
    permissions: list[str],
    operation: str,
    lifetime_seconds: int = 60,
    issued_at: int | None = None,
    private_key: rsa.RSAPrivateKey = PRIVATE_KEY,
    issuer: str = "estudio-juridico-api",
    audience: str = "ocr-api",
    subject: str = "estudio-juridico-api",
) -> str:
    now = issued_at if issued_at is not None else int(time.time())
    header = {"alg": "RS256", "typ": "JWT"}
    payload = {
        "sub": subject,
        "iss": issuer,
        "aud": audience,
        "permissions": permissions,
        "actor_user_id": "123",
        "operation": operation,
        "jti": str(uuid.uuid4()),
        "iat": now,
        "exp": now + lifetime_seconds,
    }
    encoded_header = encode_base64url(
        json.dumps(header, separators=(",", ":")).encode()
    )
    encoded_payload = encode_base64url(
        json.dumps(payload, separators=(",", ":")).encode()
    )
    signing_input = f"{encoded_header}.{encoded_payload}".encode("ascii")
    signature = private_key.sign(
        signing_input,
        padding.PKCS1v15(),
        hashes.SHA256(),
    )
    return f"{encoded_header}.{encoded_payload}.{encode_base64url(signature)}"


class InternalAuthenticationTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls) -> None:
        cls.client_context = TestClient(app)
        cls.client = cls.client_context.__enter__()

    @classmethod
    def tearDownClass(cls) -> None:
        cls.client_context.__exit__(None, None, None)
        TEMP_DIRECTORY.cleanup()

    def post_reload(self, token: str | None = None):
        headers = {"Authorization": f"Bearer {token}"} if token else {}
        return self.client.post("/admin/recargar-modelo", headers=headers)

    def test_missing_token_returns_401(self):
        self.assertEqual(self.post_reload().status_code, 401)

    def test_valid_admin_token_executes_endpoint(self):
        response = self.post_reload(create_token(["process", "admin"], "RecargarModelo"))
        self.assertEqual(response.status_code, 200)

    def test_token_cannot_be_reused(self):
        token = create_token(["admin"], "RecargarModelo")
        self.assertEqual(self.post_reload(token).status_code, 200)
        self.assertEqual(self.post_reload(token).status_code, 401)

    def test_missing_permission_returns_403(self):
        response = self.post_reload(create_token(["process"], "RecargarModelo"))
        self.assertEqual(response.status_code, 403)

    def test_wrong_operation_returns_403(self):
        response = self.post_reload(create_token(["admin"], "PrepararDataset"))
        self.assertEqual(response.status_code, 403)

    def test_expired_token_returns_401(self):
        response = self.post_reload(
            create_token(
                ["admin"],
                "RecargarModelo",
                lifetime_seconds=10,
                issued_at=int(time.time()) - 30,
            )
        )
        self.assertEqual(response.status_code, 401)

    def test_excessive_lifetime_returns_401(self):
        response = self.post_reload(
            create_token(["admin"], "RecargarModelo", lifetime_seconds=61)
        )
        self.assertEqual(response.status_code, 401)

    def test_wrong_issuer_returns_401(self):
        response = self.post_reload(
            create_token(
                ["admin"],
                "RecargarModelo",
                issuer="otra-api",
            )
        )
        self.assertEqual(response.status_code, 401)

    def test_wrong_audience_returns_401(self):
        response = self.post_reload(
            create_token(
                ["admin"],
                "RecargarModelo",
                audience="otra-api",
            )
        )
        self.assertEqual(response.status_code, 401)

    def test_wrong_subject_returns_401(self):
        response = self.post_reload(
            create_token(
                ["admin"],
                "RecargarModelo",
                subject="otro-servicio",
            )
        )
        self.assertEqual(response.status_code, 401)

    def test_wrong_signature_returns_401(self):
        other_key = rsa.generate_private_key(public_exponent=65537, key_size=2048)
        response = self.post_reload(
            create_token(["admin"], "RecargarModelo", private_key=other_key)
        )
        self.assertEqual(response.status_code, 401)


if __name__ == "__main__":
    unittest.main()
