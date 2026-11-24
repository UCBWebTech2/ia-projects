SET XACT_ABORT ON;
GO

-- Phase 2: feedback/prediction fields, model registry, category mapping

IF COL_LENGTH(N'dbo.Documentos', N'prediccionIa') IS NULL
    ALTER TABLE dbo.Documentos ADD prediccionIa VARCHAR(100) NULL;
GO
IF COL_LENGTH(N'dbo.Documentos', N'confianzaIa') IS NULL
    ALTER TABLE dbo.Documentos ADD confianzaIa FLOAT NULL;
GO
IF COL_LENGTH(N'dbo.Documentos', N'versionModelo') IS NULL
    ALTER TABLE dbo.Documentos ADD versionModelo VARCHAR(200) NULL;
GO
IF COL_LENGTH(N'dbo.Documentos', N'fechaPrediccion') IS NULL
    ALTER TABLE dbo.Documentos ADD fechaPrediccion DATETIME NULL;
GO
IF COL_LENGTH(N'dbo.Documentos', N'categoriaConfirmadaId') IS NULL
    ALTER TABLE dbo.Documentos ADD categoriaConfirmadaId INT NULL;
GO
IF COL_LENGTH(N'dbo.Documentos', N'confirmadoPor') IS NULL
    ALTER TABLE dbo.Documentos ADD confirmadoPor INT NULL;
GO
IF COL_LENGTH(N'dbo.Documentos', N'fechaConfirmacion') IS NULL
    ALTER TABLE dbo.Documentos ADD fechaConfirmacion DATETIME NULL;
GO

IF OBJECT_ID(N'dbo.ModelRegistry', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ModelRegistry (
        idModelRegistry INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        version VARCHAR(100) NOT NULL,
        modelPath VARCHAR(500) NULL,
        datasetHash VARCHAR(128) NULL,
        datasetPath VARCHAR(500) NULL,
        accuracy FLOAT NULL,
        macroF1 FLOAT NULL,
        metricsJson NVARCHAR(MAX) NULL,
        activo BIT NOT NULL CONSTRAINT DF_ModelRegistry_activo DEFAULT (0),
        fechaCreacion DATETIME NOT NULL CONSTRAINT DF_ModelRegistry_fechaCreacion DEFAULT (GETUTCDATE()),
        notas NVARCHAR(1000) NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.MapeoCategoria', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MapeoCategoria (
        idMapeoCategoria INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        modelClassName VARCHAR(100) NOT NULL,
        idCategoria INT NOT NULL,
        activo BIT NOT NULL CONSTRAINT DF_MapeoCategoria_activo DEFAULT (1),
        fechaCreacion DATETIME NULL,
        fechaModificacion DATETIME NULL,
        CONSTRAINT FK_MapeoCategoria_Categoria FOREIGN KEY (idCategoria) REFERENCES dbo.Categorias(idCategoria)
    );
    CREATE UNIQUE INDEX IX_MapeoCategoria_ModelClassName ON dbo.MapeoCategoria(modelClassName);
END;
GO
