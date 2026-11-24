# Configuración local

El repositorio no contiene credenciales. Para desarrollo, configure los valores
privados con Secret Manager de .NET desde la raíz del proyecto:

```powershell
dotnet user-secrets set "Email:UserName" "correo@ejemplo.com" --project SendEmail
dotnet user-secrets set "Email:PassWord" "contraseña-de-aplicación" --project SendEmail
dotnet user-secrets set "ConnectionStrings:CadenaSQL" "cadena-de-conexión" --project SendEmail
dotnet user-secrets set "Jwt:key" "clave-jwt-larga-y-aleatoria" --project SendEmail
dotnet user-secrets set "Supabase:Url" "https://proyecto.supabase.co" --project SendEmail
dotnet user-secrets set "Supabase:ServiceKey" "service-role-key" --project SendEmail
```

La clave privada usada para autenticar solicitudes a la API OCR debe existir en
`SendEmail/secrets/internal-jwt-private-dev.pem`. Los archivos PEM de esa carpeta
están excluidos por `.gitignore` y no deben confirmarse.

## Base de datos

Antes de guardar contraseñas con `PasswordHasher`, ejecute en SQL Server:

```powershell
sqlcmd -S "servidor" -d "base-de-datos" -E -i "Database/20260923_ampliar_contrasenia_usuario.sql"
```

Si utiliza autenticación SQL, reemplace `-E` por los parámetros de autenticación
correspondientes. El script amplía `dbo.Usuarios.contrasenia` a `VARCHAR(256)`.
