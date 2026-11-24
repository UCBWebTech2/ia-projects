SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM dbo.Categorias WHERE nombreCategoria = 'Civil')
    INSERT INTO dbo.Categorias (nombreCategoria, descripcionCategoria, idUsuario, fechaCreacion, fechaModificacion, habilitado)
    VALUES ('Civil', 'Documentos de procesos civiles', 1, GETDATE(), GETDATE(), 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Categorias WHERE nombreCategoria = 'Penal')
    INSERT INTO dbo.Categorias (nombreCategoria, descripcionCategoria, idUsuario, fechaCreacion, fechaModificacion, habilitado)
    VALUES ('Penal', 'Documentos de procesos penales', 1, GETDATE(), GETDATE(), 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Procesos WHERE nombreProceso = 'Proceso Civil - Cumplimiento de Contrato')
    INSERT INTO dbo.Procesos (nombreProceso, descripcionProceso, idUsuario, fechaCreacion, fechaModificacion, habilitado)
    VALUES ('Proceso Civil - Cumplimiento de Contrato', 'Demanda ordinaria por incumplimiento contractual', 1, GETDATE(), GETDATE(), 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Procesos WHERE nombreProceso = 'Proceso Penal - Estafa')
    INSERT INTO dbo.Procesos (nombreProceso, descripcionProceso, idUsuario, fechaCreacion, fechaModificacion, habilitado)
    VALUES ('Proceso Penal - Estafa', 'Investigación penal por delito de estafa', 1, GETDATE(), GETDATE(), 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Procesos WHERE nombreProceso = 'Proceso Penal - Lesiones Graves')
    INSERT INTO dbo.Procesos (nombreProceso, descripcionProceso, idUsuario, fechaCreacion, fechaModificacion, habilitado)
    VALUES ('Proceso Penal - Lesiones Graves', 'Querella penal por lesiones graves', 1, GETDATE(), GETDATE(), 1);

SELECT 'Categorias' as tabla, COUNT(*) as total FROM dbo.Categorias
UNION ALL SELECT 'Procesos', COUNT(*) FROM dbo.Procesos;
