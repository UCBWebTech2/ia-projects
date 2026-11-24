# Secrets

Este directorio contiene claves usadas por los servicios en Docker.

## JWT interno (API .NET <-> OCR Python)

Generar claves de desarrollo:

```bash
openssl genrsa -out internal-jwt-private-dev.pem 2048
openssl rsa -in internal-jwt-private-dev.pem -pubout -out internal-jwt-public.pem
```

Archivos esperados por `docker-compose.yml`:

- `internal-jwt-private-dev.pem`: clave privada montada en la API .NET.
- `internal-jwt-public.pem`: clave pública montada en la API OCR.

No confirmar claves privadas reales en el repositorio.
