-- Seed dev admin user (idempotent). Password: mr12345
-- Hash format: ASP.NET Core Identity PasswordHasher<Usuario> v3 (PBKDF2-HMAC-SHA256).

SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE correoElectronico = 'admin@estudiojuridico.com')
    BEGIN
        INSERT INTO dbo.Usuarios (
            nombreUsuario,
            correoElectronico,
            contrasenia,
            rol,
            restablecer,
            confirmar,
            Token,
            fechaCreacion,
            fechaModificacion
        )
        VALUES (
            'Admin',
            'admin@estudiojuridico.com',
            'AQAAAAIAAYagAAAAELUXxlq5cq9CiNxOBDODpj5TH1gHSxa3PddAglSBOQ7f8eA3mPj0UKWm8AxXXrPgPg==',
            'Administrador',
            0,
            1,
            NULL,
            GETDATE(),
            GETDATE()
        );
    END
    ELSE
    BEGIN
        UPDATE dbo.Usuarios
        SET contrasenia = 'AQAAAAIAAYagAAAAELUXxlq5cq9CiNxOBDODpj5TH1gHSxa3PddAglSBOQ7f8eA3mPj0UKWm8AxXXrPgPg==',
            confirmar = 1,
            rol = 'Administrador',
            restablecer = 0,
            fechaModificacion = GETDATE()
        WHERE correoElectronico = 'admin@estudiojuridico.com';
    END
END;
GO