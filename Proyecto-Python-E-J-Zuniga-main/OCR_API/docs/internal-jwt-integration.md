# Integración JWT interno .NET 8 - Python OCR

La API Python acepta exclusivamente JWT internos RS256 emitidos por la API .NET.
Cada token autoriza una sola operación y puede utilizarse una sola vez.

## Claims obligatorios

```json
{
  "sub": "estudio-juridico-api",
  "iss": "estudio-juridico-api",
  "aud": "ocr-api",
  "permissions": ["process"],
  "actor_user_id": "123",
  "operation": "SubirArchivo",
  "jti": "identificador-unico",
  "iat": 1781299940,
  "exp": 1781300000
}
```

- Algoritmo obligatorio: `RS256`.
- Duración máxima predeterminada: 60 segundos.
- `jti` debe ser único para cada llamada.
- La clave privada permanece exclusivamente en .NET.
- Python recibe únicamente la clave pública PEM.

## Operaciones y permisos

| Endpoint | Permiso | operation |
|---|---|---|
| `POST /SubirArchivo` | `process` | `SubirArchivo` |
| `POST /SubirArchivoPaddle` | `process` | `SubirArchivoPaddle` |
| `POST /SubirArchivoClasificado` | `process` | `SubirArchivoClasificado` |
| `POST /ExtraerEntidades` | `process` | `ExtraerEntidades` |
| `POST /PrepararDataset` | `admin` | `PrepararDataset` |
| `POST /ProbarPaddleOCRImagen` | `admin` | `ProbarPaddleOCRImagen` |
| `POST /ProbarOCRAutoImagen` | `admin` | `ProbarOCRAutoImagen` |
| `POST /CompararOCR` | `admin` | `CompararOCR` |
| `POST /CompararOCRImagen` | `admin` | `CompararOCRImagen` |
| `POST /admin/recargar-modelo` | `admin` | `RecargarModelo` |

## Endpoints sin JWT interno

| Endpoint | Motivo |
|---|---|
| `POST /ProbarClasificador` | Endpoint de prueba del clasificador Civil/Penal. |

## Configuración Python

```env
INTERNAL_JWT_PUBLIC_KEY_PATH=secrets/internal-jwt-public.pem
INTERNAL_JWT_ISSUER=estudio-juridico-api
INTERNAL_JWT_AUDIENCE=ocr-api
INTERNAL_JWT_SUBJECT=estudio-juridico-api
INTERNAL_JWT_ALGORITHM=RS256
INTERNAL_JWT_MAX_LIFETIME_SECONDS=60
INTERNAL_JWT_CLOCK_SKEW_SECONDS=5
```

## Comportamiento

- Sin token o token inválido: `401 Unauthorized`.
- Token válido sin permiso o para otra operación: `403 Forbidden`.
- Clave pública ausente o inválida: `503 Service Unavailable`.
- Token reutilizado: `401 Unauthorized`.

La API .NET debe generar un token nuevo para cada llamada y enviarlo mediante:

```http
Authorization: Bearer <jwt-interno>
```

## Responsabilidades de la API .NET

1. Conservar la clave privada RSA fuera del repositorio y fuera de Python.
2. Entregar a Python únicamente la clave pública PEM correspondiente.
3. Autorizar primero al usuario según sus roles `Abogado` o `Administrador`.
4. Convertir esa autorización a permiso `process` o `admin`.
5. Generar un JWT nuevo inmediatamente antes de cada llamada a Python.
6. Asignar la operación exacta indicada en la tabla.
7. Generar un `jti` nuevo, por ejemplo mediante `Guid.NewGuid()`.
8. Utilizar fechas UTC y claims `iat`/`exp` en formato NumericDate.
9. Establecer una duración máxima de 60 segundos.
10. Generar otro token si debe reintentar una llamada.
11. No reenviar a Python el JWT público utilizado por el usuario.
12. No registrar la clave privada ni el token interno completo.

La protección contra reutilización se mantiene en memoria dentro del proceso
Python. Si en el futuro se ejecutan varias instancias o workers de FastAPI,
debe sustituirse por un registro compartido, por ejemplo Redis.
