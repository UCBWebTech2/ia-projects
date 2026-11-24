SET NOCOUNT ON;
UPDATE dbo.Documentos SET contenidoDocumento=N'ESTUDIO JURÍDICO ZÚÑIGA
Juzgado Público Civil y Comercial - Santa Cruz de la Sierra
CONTESTACIÓN A DEMANDA ORDINARIA
______________________________________________________________________________
SEÑOR JUEZ PÚBLICO EN LO CIVIL Y COMERCIAL:
Roberto Suárez Peña, en representación legal de Constructora Altiplano S.R.L., dentro del proceso ordinario de cumplimiento de contrato seguido por Carlos Alberto Méndez Rojas, responde a la demanda en los siguientes términos:
I. NEGATIVA GENERAL Y ESPECÍFICA
Niego de forma general y específica todos los hechos expuestos en la demanda, salvo aquellos que sean expresamente reconocidos en la presente contestación.
II. HECHOS
1. Es cierto que se suscribió el Contrato de Construcción en fecha 15 de marzo de 2025; sin embargo, el demandante incumplió con los pagos parciales acordados en las cláusulas cuarta y quinta.
2. La demora en la ejecución de la obra fue consecuencia directa de la falta de pago oportuno por parte del demandante, lo cual constituye excepción de contrato no cumplido.
III. FUNDAMENTOS DE DERECHO
Invoco los artículos 568 y 569 del Código Civil, que establecen la excepción de contrato no cumplido, así como el principio de buena fe contractual.
IV. PETITORIO
Solicito: 1) Rechazar la demanda por improbada; 2) Declarar la excepción de contrato no cumplido; 3) Imponer costas al demandante.
Santa Cruz de la Sierra, 25 de abril de 2026.
Abg. Jorge Luis Camacho
Matrícula C.A.S.C. 5432', fechaModificacion=GETDATE() WHERE idDoc=2;
DELETE FROM dbo.Indices WHERE idDoc=2;
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (2,N'artículos 568 y 569',N'ARTICULO',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (2,N'Juzgado Público Civil y Comercial',N'INSTITUCION',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (2,N'Santa Cruz de la Sierra',N'LOC',2);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (2,N'Roberto Suárez Peña',N'PER',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (2,N'Constructora Altiplano S.R.L',N'ORG',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (2,N'Carlos Alberto Méndez Rojas',N'PER',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (2,N'Jorge Luis Camacho',N'PER',1);