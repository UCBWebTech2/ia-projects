SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
BEGIN
    THROW 50001, 'No existe la tabla dbo.Usuarios.', 1;
END;

IF COL_LENGTH(N'dbo.Usuarios', N'contrasenia') IS NULL
BEGIN
    THROW 50002, 'No existe la columna dbo.Usuarios.contrasenia.', 1;
END;

ALTER TABLE dbo.Usuarios
    ALTER COLUMN contrasenia VARCHAR(256) NULL;

COMMIT TRANSACTION;
