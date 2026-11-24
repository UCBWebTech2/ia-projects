SET NOCOUNT ON;
UPDATE dbo.Documentos SET contenidoDocumento=N'ESTUDIO JURÍDICO ZÚÑIGA
Juzgado de Instrucción en lo Penal - Cochabamba
QUERELLA PENAL POR LESIONES GRAVES
______________________________________________________________________________
SEÑOR JUEZ DE INSTRUCCIÓN EN LO PENAL:
I. DATOS DEL QUERELLANTE
Miguel Ángel Flores Gutiérrez, con C.I. 5234567 CB, domiciliado en Av. América N° 321, Cochabamba.
II. DATOS DEL QUERELLADO
Pedro Luis Mamani Condori, con C.I. 7345678 CB, con domicilio conocido en Zona Sud, Cochabamba.
III. RELACIÓN DE LOS HECHOS
El día 2 de febrero de 2026, aproximadamente a horas 22:30, en las inmediaciones de la Av. Ayacucho, el querellado agredió físicamente al querellante con un objeto contundente, ocasionándole lesiones de consideración.
El Certificado Médico Forense N° 123/2026 establece una incapacidad de 25 días, calificando las lesiones como graves conforme a la normativa penal vigente.
IV. FUNDAMENTOS JURÍDICOS
La conducta se subsume en el delito de Lesiones Graves previsto en el artículo 271 del Código Penal boliviano. Se solicita la aplicación de medidas cautelares conforme al artículo 231 del Código Procesal Penal.
V. PETITORIO
Solicito: 1) Admisión de la querella; 2) Imputación formal; 3) Imposición de medidas cautelares; 4) Reparación del daño civil.
Cochabamba, 20 de febrero de 2026.
Abg. Laura Patricia Montaño
Matrícula C.A.C. 3344', fechaModificacion=GETDATE() WHERE idDoc=5;
DELETE FROM dbo.Indices WHERE idDoc=5;
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (5,N'artículo 271',N'ARTICULO',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (5,N'artículo 231',N'ARTICULO',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (5,N'Juzgado de Instrucción en lo Penal',N'INSTITUCION',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (5,N'Cochabamba',N'LOC',4);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (5,N'Miguel Ángel Flores Gutiérrez',N'PER',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (5,N'Av. América N',N'LOC',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (5,N'Pedro Luis Mamani Condori',N'PER',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (5,N'Zona Sud',N'LOC',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (5,N'Av. Ayacucho',N'LOC',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (5,N'Código Penal',N'INSTRUMENTO',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (5,N'Laura Patricia Montaño',N'PER',1);