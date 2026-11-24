SET XACT_ABORT ON;
GO

-- 004_feature_expansion.sql
-- Expansión funcional: versionado, tags, archivado, búsquedas, notificaciones, reportes, métricas.
-- Idempotente; ejecutar en contexto de BDProyecto.

-- Documentos: campos de clasificación/gestión
IF COL_LENGTH(N'dbo.Documentos', N'abogadoResponsableId') IS NULL
    ALTER TABLE dbo.Documentos ADD abogadoResponsableId INT NULL;
GO
IF COL_LENGTH(N'dbo.Documentos', N'clienteNombre') IS NULL
    ALTER TABLE dbo.Documentos ADD clienteNombre NVARCHAR(200) NULL;
GO
IF COL_LENGTH(N'dbo.Documentos', N'estadoDocumento') IS NULL
    ALTER TABLE dbo.Documentos ADD estadoDocumento VARCHAR(30) NOT NULL CONSTRAINT DF_Documentos_estadoDocumento DEFAULT ('ACTIVO'); -- ACTIVO|ARCHIVADO|ELIMINADO_LOGICO
GO
IF COL_LENGTH(N'dbo.Documentos', N'versionActual') IS NULL
    ALTER TABLE dbo.Documentos ADD versionActual INT NOT NULL CONSTRAINT DF_Documentos_versionActual DEFAULT (1);
GO
IF COL_LENGTH(N'dbo.Documentos', N'hashContenido') IS NULL
    ALTER TABLE dbo.Documentos ADD hashContenido VARCHAR(128) NULL;
GO

IF OBJECT_ID(N'dbo.FK_Documentos_AbogadoResponsable', N'F') IS NULL
    AND COL_LENGTH(N'dbo.Documentos', N'abogadoResponsableId') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Documentos
        ADD CONSTRAINT FK_Documentos_AbogadoResponsable FOREIGN KEY (abogadoResponsableId) REFERENCES dbo.Usuarios(idUsuario);
END;
GO

-- Versionado documental
IF OBJECT_ID(N'dbo.DocumentoVersiones', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DocumentoVersiones (
        idVersion INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        idDoc INT NOT NULL,
        numeroVersion INT NOT NULL,
        nombreDocumento VARCHAR(255) NULL,
        contenidoDocumento NVARCHAR(MAX) NULL,
        rutaDocumento VARCHAR(255) NULL,
        tipoDocumento VARCHAR(50) NULL,
        idCategoria INT NULL,
        idProceso INT NULL,
        idUsuarioModifico INT NULL,
        comentario NVARCHAR(500) NULL,
        fechaVersion DATETIME NOT NULL CONSTRAINT DF_DocumentoVersiones_fecha DEFAULT (GETUTCDATE()),
        hashContenido VARCHAR(128) NULL,
        CONSTRAINT FK_DocumentoVersiones_Doc FOREIGN KEY (idDoc) REFERENCES dbo.Documentos(idDoc),
        CONSTRAINT FK_DocumentoVersiones_Usuario FOREIGN KEY (idUsuarioModifico) REFERENCES dbo.Usuarios(idUsuario)
    );
    CREATE INDEX IX_DocumentoVersiones_Doc ON dbo.DocumentoVersiones(idDoc, numeroVersion DESC);
END;
GO

-- Etiquetas/tags
IF OBJECT_ID(N'dbo.Tags', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tags (
        idTag INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        nombre VARCHAR(100) NOT NULL,
        fechaCreacion DATETIME NOT NULL CONSTRAINT DF_Tags_fecha DEFAULT (GETUTCDATE())
    );
    CREATE UNIQUE INDEX UX_Tags_nombre ON dbo.Tags(nombre);
END;
GO

IF OBJECT_ID(N'dbo.DocumentoTags', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DocumentoTags (
        idDoc INT NOT NULL,
        idTag INT NOT NULL,
        fechaAsignacion DATETIME NOT NULL CONSTRAINT DF_DocumentoTags_fecha DEFAULT (GETUTCDATE()),
        CONSTRAINT PK_DocumentoTags PRIMARY KEY (idDoc, idTag),
        CONSTRAINT FK_DocumentoTags_Doc FOREIGN KEY (idDoc) REFERENCES dbo.Documentos(idDoc),
        CONSTRAINT FK_DocumentoTags_Tag FOREIGN KEY (idTag) REFERENCES dbo.Tags(idTag)
    );
END;
GO

-- Búsquedas y actividad (métricas/dashboard)
IF OBJECT_ID(N'dbo.Busquedas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Busquedas (
        idBusqueda INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        idUsuario INT NULL,
        tipoBusqueda VARCHAR(50) NOT NULL, -- CONTENIDO, FILTROS, ENTIDAD, NER, CLIENTE, ABOGADO
        query NVARCHAR(1000) NULL,
        filtrosJson NVARCHAR(MAX) NULL,
        resultados INT NULL,
        tiempoMs INT NULL,
        fecha DATETIME NOT NULL CONSTRAINT DF_Busquedas_fecha DEFAULT (GETUTCDATE()),
        CONSTRAINT FK_Busquedas_Usuario FOREIGN KEY (idUsuario) REFERENCES dbo.Usuarios(idUsuario)
    );
    CREATE INDEX IX_Busquedas_Fecha ON dbo.Busquedas(fecha DESC);
    CREATE INDEX IX_Busquedas_Usuario ON dbo.Busquedas(idUsuario, fecha DESC);
END;
GO

-- Clasificaciones/feedback auditables
IF OBJECT_ID(N'dbo.ClasificacionHistorial', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ClasificacionHistorial (
        idHistorial INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        idDoc INT NOT NULL,
        categoriaAnteriorId INT NULL,
        categoriaNuevaId INT NULL,
        prediccionIa VARCHAR(100) NULL,
        confianzaIa FLOAT NULL,
        versionModelo VARCHAR(200) NULL,
        origen VARCHAR(30) NOT NULL, -- IA, USUARIO, REGLA, RETRAIN
        idUsuario INT NULL,
        fecha DATETIME NOT NULL CONSTRAINT DF_ClasificacionHistorial_fecha DEFAULT (GETUTCDATE()),
        comentario NVARCHAR(500) NULL,
        CONSTRAINT FK_ClasificacionHistorial_Doc FOREIGN KEY (idDoc) REFERENCES dbo.Documentos(idDoc),
        CONSTRAINT FK_ClasificacionHistorial_Usuario FOREIGN KEY (idUsuario) REFERENCES dbo.Usuarios(idUsuario)
    );
    CREATE INDEX IX_ClasificacionHistorial_Doc ON dbo.ClasificacionHistorial(idDoc, fecha DESC);
END;
GO

-- Notificaciones
IF OBJECT_ID(N'dbo.Notificaciones', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notificaciones (
        idNotificacion INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        idUsuario INT NULL,
        tipo VARCHAR(50) NOT NULL, -- OCR_COMPLETADO, CLASIFICACION, DOCUMENTO, SISTEMA
        titulo NVARCHAR(200) NOT NULL,
        mensaje NVARCHAR(1000) NULL,
        leida BIT NOT NULL CONSTRAINT DF_Notificaciones_leida DEFAULT (0),
        enviadaCorreo BIT NOT NULL CONSTRAINT DF_Notificaciones_correo DEFAULT (0),
        fechaCreacion DATETIME NOT NULL CONSTRAINT DF_Notificaciones_fecha DEFAULT (GETUTCDATE()),
        metadataJson NVARCHAR(MAX) NULL,
        CONSTRAINT FK_Notificaciones_Usuario FOREIGN KEY (idUsuario) REFERENCES dbo.Usuarios(idUsuario)
    );
    CREATE INDEX IX_Notificaciones_Usuario ON dbo.Notificaciones(idUsuario, leida, fechaCreacion DESC);
END;
GO

-- Entidades NER normalizadas (complementa Indices para búsqueda avanzada)
IF OBJECT_ID(N'dbo.DocumentoEntidades', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DocumentoEntidades (
        idEntidad INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        idDoc INT NOT NULL,
        tipoEntidad VARCHAR(50) NOT NULL, -- DEMANDANTE, DEMANDADO, JUEZ, FISCAL, ABOGADO, NRO_PROCESO, JUZGADO, FECHA_AUDIENCIA, PER, LOC...
        valor NVARCHAR(300) NOT NULL,
        valorNormalizado NVARCHAR(300) NULL,
        fuente VARCHAR(30) NOT NULL CONSTRAINT DF_DocumentoEntidades_fuente DEFAULT ('NER'),
        confianza FLOAT NULL,
        fechaCreacion DATETIME NOT NULL CONSTRAINT DF_DocumentoEntidades_fecha DEFAULT (GETUTCDATE()),
        CONSTRAINT FK_DocumentoEntidades_Doc FOREIGN KEY (idDoc) REFERENCES dbo.Documentos(idDoc)
    );
    CREATE INDEX IX_DocumentoEntidades_Doc ON dbo.DocumentoEntidades(idDoc, tipoEntidad);
    CREATE INDEX IX_DocumentoEntidades_TipoValor ON dbo.DocumentoEntidades(tipoEntidad, valorNormalizado);
END;
GO

-- Jobs de procesamiento masivo/reentrenamiento
IF OBJECT_ID(N'dbo.JobsProcesamiento', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.JobsProcesamiento (
        idJob INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        tipo VARCHAR(50) NOT NULL, -- OCR_MASIVO, REENTRENAMIENTO, EXPORT_REPORTE
        estado VARCHAR(30) NOT NULL, -- PENDIENTE, EJECUTANDO, COMPLETADO, ERROR
        idUsuario INT NULL,
        parametrosJson NVARCHAR(MAX) NULL,
        resultadoJson NVARCHAR(MAX) NULL,
        fechaCreacion DATETIME NOT NULL CONSTRAINT DF_Jobs_fecha DEFAULT (GETUTCDATE()),
        fechaInicio DATETIME NULL,
        fechaFin DATETIME NULL,
        CONSTRAINT FK_Jobs_Usuario FOREIGN KEY (idUsuario) REFERENCES dbo.Usuarios(idUsuario)
    );
    CREATE INDEX IX_Jobs_Estado ON dbo.JobsProcesamiento(estado, fechaCreacion DESC);
END;
GO