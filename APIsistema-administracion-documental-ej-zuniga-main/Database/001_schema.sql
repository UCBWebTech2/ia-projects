SET XACT_ABORT ON;
GO

IF DB_ID() IS NULL
BEGIN
    THROW 50000, 'Debe ejecutarse en el contexto de una base de datos.', 1;
END;
GO

IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios (
        idUsuario INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        nombreUsuario VARCHAR(70) NULL,
        correoElectronico VARCHAR(50) NULL,
        contrasenia VARCHAR(256) NULL,
        rol VARCHAR(50) NULL,
        restablecer BIT NULL,
        confirmar BIT NULL,
        Token VARCHAR(256) NULL,
        fechaCreacion DATETIME NULL,
        fechaModificacion DATETIME NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.Categorias', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categorias (
        idCategoria INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        nombreCategoria VARCHAR(255) NULL,
        descripcionCategoria VARCHAR(255) NULL,
        idUsuario INT NOT NULL,
        fechaCreacion DATETIME NULL,
        fechaModificacion DATETIME NULL,
        habilitado BIT NOT NULL CONSTRAINT DF_Categorias_habilitado DEFAULT (1),
        CONSTRAINT FK_Categoria_Usuario FOREIGN KEY (idUsuario) REFERENCES dbo.Usuarios(idUsuario)
    );
END;
GO

IF OBJECT_ID(N'dbo.Procesos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Procesos (
        idProceso INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        nombreProceso VARCHAR(255) NULL,
        descripcionProceso VARCHAR(500) NULL,
        idUsuario INT NOT NULL,
        fechaCreacion DATETIME NULL,
        fechaModificacion DATETIME NULL,
        habilitado BIT NOT NULL CONSTRAINT DF_Procesos_habilitado DEFAULT (1),
        CONSTRAINT FK_Proceso_Usuario FOREIGN KEY (idUsuario) REFERENCES dbo.Usuarios(idUsuario)
    );
END;
GO

IF OBJECT_ID(N'dbo.Documentos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Documentos (
        idDoc INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        nombreDocumento VARCHAR(255) NULL,
        contenidoDocumento NVARCHAR(MAX) NULL,
        rutaDocumento VARCHAR(255) NULL,
        tipoDocumento VARCHAR(50) NULL,
        fechaCreacion DATETIME NULL,
        fechaModificacion DATETIME NULL,
        idUsuario INT NULL,
        idProceso INT NULL,
        idCategoria INT NULL,
        habilitado BIT NOT NULL CONSTRAINT DF_Documentos_habilitado DEFAULT (1),
        CONSTRAINT FK_Documento_Usuario FOREIGN KEY (idUsuario) REFERENCES dbo.Usuarios(idUsuario),
        CONSTRAINT FK_Documento_Proceso FOREIGN KEY (idProceso) REFERENCES dbo.Procesos(idProceso),
        CONSTRAINT FK_Documento_Categoria FOREIGN KEY (idCategoria) REFERENCES dbo.Categorias(idCategoria)
    );
END;
GO

IF OBJECT_ID(N'dbo.Indices', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Indices (
        idIndice INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        idDoc INT NULL,
        termino VARCHAR(150) NULL,
        tipoTermino VARCHAR(50) NULL,
        frecuencia INT NULL,
        CONSTRAINT FK_Indices_Documento FOREIGN KEY (idDoc) REFERENCES dbo.Documentos(idDoc)
    );
END;
GO

IF OBJECT_ID(N'dbo.OCR', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OCR (
        idOCR INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        idDoc INT NULL,
        rutaDocumento VARCHAR(255) NULL,
        fechaProceso DATETIME NULL,
        fechaModificacion DATETIME NULL,
        CONSTRAINT FK_OCR_Documento FOREIGN KEY (idDoc) REFERENCES dbo.Documentos(idDoc)
    );
END;
GO

IF OBJECT_ID(N'dbo.Auditoria', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Auditoria (
        idAuditoria INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        idUsuario INT NULL,
        accion VARCHAR(255) NULL,
        descripcion VARCHAR(255) NULL,
        fecha DATETIME NULL,
        CONSTRAINT FK_Auditoria_Usuario FOREIGN KEY (idUsuario) REFERENCES dbo.Usuarios(idUsuario)
    );
END;
GO
