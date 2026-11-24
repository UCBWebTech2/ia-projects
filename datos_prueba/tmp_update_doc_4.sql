SET NOCOUNT ON;
UPDATE dbo.Documentos SET contenidoDocumento=N'ESTUDIO JURÍDICO ZÚÑIGA
Ministerio Público - Fiscalía Departamental de La Paz
DENUNCIA PENAL POR ESTAFA
______________________________________________________________________________
SEÑOR FISCAL DE MATERIA:
I. DATOS DEL DENUNCIANTE
Ana Lucía Vargas Torrez, con C.I. 6123456 LP, mayor de edad, domiciliada en Calle Murillo N° 789, La Paz, interpone formal denuncia penal contra Juan Carlos Quispe Mamani por la presunta comisión del delito de Estafa.
II. DATOS DEL DENUNCIADO
Juan Carlos Quispe Mamani, con C.I. 4987654 LP, domiciliado en Zona Villa Fátima, Calle 2 N° 100, La Paz.
III. HECHOS
1. En fecha 5 de enero de 2026, el denunciado se presentó como representante de una empresa de inversiones y ofreció a la denunciante un rendimiento del 20% mensual.
2. La denunciante entregó la suma de Bs. 80.000.- (Ochenta Mil 00/100 Bolivianos) mediante transferencia bancaria y efectivo, según comprobantes adjuntos.
3. Desde el 20 de febrero de 2026, el denunciado dejó de responder llamadas y se constató que la oficina indicada no existe.
IV. TIPIFICACIÓN
Los hechos descritos se adecuan al tipo penal de Estafa previsto en el artículo 335 del Código Penal boliviano, al haber inducido a error a la víctima para obtener un beneficio económico indebido.
V. PETITORIO
Solicito: 1) Recepción de la denuncia; 2) Inicio de investigación penal; 3) Citación del denunciado; 4) Secuestro de documentación bancaria; 5) Cautela de bienes del denunciado.
La Paz, 15 de marzo de 2026.
Ana Lucía Vargas Torrez
C.I. 6123456 LP', fechaModificacion=GETDATE() WHERE idDoc=4;
DELETE FROM dbo.Indices WHERE idDoc=4;
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (4,N'artículo 335',N'ARTICULO',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (4,N'Ministerio Público',N'INSTITUCION',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (4,N'Fiscalía Departamental de La Paz',N'INSTITUCION',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (4,N'Ana Lucía Vargas Torrez',N'PER',2);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (4,N'Calle Murillo N',N'LOC',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (4,N'La Paz',N'LOC',3);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (4,N'Juan Carlos Quispe Mamani',N'PER',2);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (4,N'Zona Villa Fátima',N'LOC',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (4,N'Calle 2 N° 100',N'LOC',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (4,N'Código Penal',N'INSTRUMENTO',1);