SET NOCOUNT ON;
UPDATE dbo.Documentos SET contenidoDocumento=N'ESTUDIO JURÍDICO ZÚÑIGA
Juzgado Público Civil y Comercial - Santa Cruz de la Sierra
DEMANDA ORDINARIA DE CUMPLIMIENTO DE CONTRATO
______________________________________________________________________________
SEÑOR JUEZ PÚBLICO EN LO CIVIL Y COMERCIAL:
I. DATOS DE LAS PARTES
DEMANDANTE: Carlos Alberto Méndez Rojas, con C.I. 3456789 SC, mayor de edad, hábil por derecho, domiciliado en la Av. Banzer N° 450, Santa Cruz de la Sierra.
DEMANDADO: Constructora Altiplano S.R.L., representada legalmente por su Gerente General Ing. Roberto Suárez Peña, con domicilio legal en Calle Warnes N° 120, Santa Cruz de la Sierra.
II. OBJETO DE LA DEMANDA
El demandante interpone demanda ordinaria de cumplimiento de contrato, más daños y perjuicios, contra Constructora Altiplano S.R.L., por incumplimiento del Contrato de Construcción suscrito en fecha 15 de marzo de 2025.
III. HECHOS
1. En fecha 15 de marzo de 2025, las partes suscribieron un Contrato de Construcción para la edificación de una vivienda unifamiliar en el Barrio Equipetrol, por el monto total de Bs. 450.000.- (Cuatrocientos Cincuenta Mil 00/100 Bolivianos).
2. El plazo de entrega fue pactado para el 15 de septiembre de 2025, habiendo transcurrido más de seis meses sin que la obra haya sido concluida.
3. A pesar de los requerimientos escritos enviados en fechas 20 de octubre de 2025 y 10 de noviembre de 2025, la parte demandada no cumplió con sus obligaciones contractuales.
IV. FUNDAMENTOS DE DERECHO
Se funda la presente demanda en los artículos 519, 568 y 574 del Código Civil boliviano, que establecen la obligación de cumplir los contratos y la responsabilidad por daños y perjuicios derivados del incumplimiento.
V. PETITORIO
Solicito a su autoridad: 1) Admitir la demanda; 2) Ordenar el cumplimiento del contrato o la devolución de los montos pagados; 3) Condenar al pago de daños y perjuicios; 4) Imponer costas y costos.
Santa Cruz de la Sierra, 10 de abril de 2026.
Abg. María Fernanda Zúñiga
Matrícula C.A.S.C. 9876', fechaModificacion=GETDATE() WHERE idDoc=1;
DELETE FROM dbo.Indices WHERE idDoc=1;
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (1,N'artículos 519, 568 y 574',N'ARTICULO',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (1,N'Juzgado Público Civil y Comercial',N'INSTITUCION',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (1,N'Santa Cruz de la Sierra',N'LOC',4);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (1,N'Carlos Alberto Méndez Rojas',N'PER',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (1,N'Av. Banzer N',N'LOC',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (1,N'Roberto Suárez Peña',N'PER',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (1,N'Calle Warnes N',N'LOC',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (1,N'Constructora Altiplano S.R.L',N'ORG',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (1,N'Barrio Equipetrol',N'LOC',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (1,N'María Fernanda Zúñiga',N'PER',1);