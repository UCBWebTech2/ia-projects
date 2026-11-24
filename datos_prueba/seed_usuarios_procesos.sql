SET NOCOUNT ON;

-- Actualizar procesos con nombres de litigantes
UPDATE dbo.Procesos
SET nombreProceso = 'Proceso Civil - Méndez Rojas c/ Constructora Altiplano S.R.L.',
    descripcionProceso = 'Demanda ordinaria de cumplimiento de contrato. Demandante: Carlos Alberto Méndez Rojas. Demandado: Constructora Altiplano S.R.L.',
    fechaModificacion = GETDATE()
WHERE idProceso = 1;

UPDATE dbo.Procesos
SET nombreProceso = 'Proceso Penal - Estafa en perjuicio de Vargas Torrez',
    descripcionProceso = 'Investigación penal por delito de estafa. Víctima: Ana Lucía Vargas Torrez. Imputado: Juan Carlos Quispe Mamani.',
    fechaModificacion = GETDATE()
WHERE idProceso = 2;

UPDATE dbo.Procesos
SET nombreProceso = 'Proceso Penal - Lesiones Graves Flores Gutiérrez',
    descripcionProceso = 'Querella penal por lesiones graves. Querellante: Miguel Ángel Flores Gutiérrez. Querellado: Pedro Luis Mamani Condori.',
    fechaModificacion = GETDATE()
WHERE idProceso = 3;

-- Usuarios de prueba (password: mr12345, hash ASP.NET Core Identity v3)
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE correoElectronico='maria.zuniga@estudiojuridico.com')
INSERT INTO dbo.Usuarios (nombreUsuario,correoElectronico,contrasenia,rol,restablecer,confirmar,Token,fechaCreacion,fechaModificacion)
VALUES ('María Fernanda Zúñiga','maria.zuniga@estudiojuridico.com','AQEAAACghgEAEAAAAB9G0/yWaiqUEMUlcGtQN4Ai1J8NtDY9dkiCaWXrWoBnd/VPSyrS1/eFc7or0Zwqpg==','Administrador',0,1,NULL,GETDATE(),GETDATE());

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE correoElectronico='jorge.camacho@estudiojuridico.com')
INSERT INTO dbo.Usuarios (nombreUsuario,correoElectronico,contrasenia,rol,restablecer,confirmar,Token,fechaCreacion,fechaModificacion)
VALUES ('Jorge Luis Camacho','jorge.camacho@estudiojuridico.com','AQEAAACghgEAEAAAAIpWeMURz4imA2nwlvx+LT7h1wqohZ8hGXk3GKnl+65tQQCxPzGVsNNd8+jn4cOUDg==','Abogado',0,1,NULL,GETDATE(),GETDATE());

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE correoElectronico='laura.montano@estudiojuridico.com')
INSERT INTO dbo.Usuarios (nombreUsuario,correoElectronico,contrasenia,rol,restablecer,confirmar,Token,fechaCreacion,fechaModificacion)
VALUES ('Laura Patricia Montaño','laura.montano@estudiojuridico.com','AQEAAACghgEAEAAAAKx14rIcShB52vs/+0CDmW4G3nYSTyv1dsrnULAUcaeRzJvNSKYPSqw2L6s6RFd+lA==','Abogado',0,1,NULL,GETDATE(),GETDATE());

SELECT idProceso,nombreProceso,descripcionProceso FROM dbo.Procesos ORDER BY idProceso;
SELECT idUsuario,nombreUsuario,correoElectronico,rol,confirmar FROM dbo.Usuarios ORDER BY idUsuario;
