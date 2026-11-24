SET NOCOUNT ON;
UPDATE dbo.Documentos SET contenidoDocumento=N'ESTUDIO JURÍDICO ZÚÑIGA
Juzgado Público Civil y Comercial N° 3 - Santa Cruz de la Sierra
SENTENCIA CIVIL N° 045/2026
______________________________________________________________________________
PROCESO: Ordinario de Cumplimiento de Contrato
DEMANDANTE: Carlos Alberto Méndez Rojas
DEMANDADO: Constructora Altiplano S.R.L.
VISTOS: La demanda, contestación, pruebas ofrecidas y producidas, y todo cuanto de autos consta;
CONSIDERANDO:
I. Que, de la revisión de los antecedentes procesales, se evidencia la existencia del Contrato de Construcción suscrito el 15 de marzo de 2025, documento que no fue objetado por la parte demandada.
II. Que, la parte demandante ha demostrado el incumplimiento del plazo pactado, mientras que la parte demandada no acreditó de forma suficiente la excepción de contrato no cumplido.
III. Que, conforme a los artículos 519 y 574 del Código Civil, corresponde disponer el cumplimiento de la obligación contractual y la reparación de daños probados.
POR TANTO:
La Juez Pública en lo Civil y Comercial N° 3, administrando justicia en nombre del Estado Plurinacional de Bolivia, FALLA: Declarando PROBADA en parte la demanda ordinaria interpuesta por Carlos Alberto Méndez Rojas contra Constructora Altiplano S.R.L., disponiendo el cumplimiento del contrato en el plazo de 60 días calendario, bajo conminatoria de ejecución forzosa.
Regístrese, notifíquese y cúmplase.
Dra. Patricia Ríos Mendoza
Jueza Pública Civil y Comercial N° 3', fechaModificacion=GETDATE() WHERE idDoc=3;
DELETE FROM dbo.Indices WHERE idDoc=3;
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (3,N'artículos 519 y 574',N'ARTICULO',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (3,N'Juzgado Público Civil y Comercial',N'INSTITUCION',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (3,N'Santa Cruz de la Sierra',N'LOC',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (3,N'Carlos Alberto Méndez Rojas',N'PER',2);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (3,N'Estado Plurinacional de Bolivia',N'LOC',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (3,N'Constructora Altiplano S.R.L',N'ORG',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (3,N'Dra. Patricia Ríos Mendoza',N'PER',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (3,N'Jueza Pública Civil',N'PER',1);