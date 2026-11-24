SET NOCOUNT ON;
UPDATE dbo.Documentos SET contenidoDocumento=N'ESTUDIO JURÍDICO ZÚÑIGA
Tribunal de Sentencia Penal N° 1 - La Paz
SENTENCIA PENAL N° 112/2026
______________________________________________________________________________
PROCESO: Penal público por el delito de Estafa
ACUSADO: Juan Carlos Quispe Mamani
VÍCTIMA: Ana Lucía Vargas Torrez
VISTOS Y CONSIDERANDO:
Que, durante el juicio oral, público y contradictorio, se produjo la prueba testimonial, documental y pericial ofrecida por el Ministerio Público y la víctima, acreditándose que el acusado obtuvo la suma de Bs. 80.000.- mediante engaño.
Que, la defensa no logró desvirtuar los elementos constitutivos del tipo penal previsto en el artículo 335 del Código Penal.
POR TANTO:
El Tribunal de Sentencia Penal N° 1, administrando justicia en nombre del Estado Plurinacional de Bolivia, declara a Juan Carlos Quispe Mamani AUTOR y CULPABLE del delito de Estafa, imponiendo la pena de cinco (5) años de presidio, más el pago de responsabilidad civil a favor de la víctima.
Regístrese, notifíquese y ejecútese conforme a ley.
Dr. Ramiro Castillo Vega
Presidente del Tribunal de Sentencia Penal N° 1', fechaModificacion=GETDATE() WHERE idDoc=6;
DELETE FROM dbo.Indices WHERE idDoc=6;
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (6,N'artículo 335',N'ARTICULO',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (6,N'Tribunal de Sentencia Penal',N'INSTITUCION',3);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (6,N'La Paz',N'LOC',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (6,N'Juan Carlos Quispe Mamani',N'PER',2);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (6,N'Ana Lucía Vargas Torrez',N'PER',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (6,N'Ministerio Público',N'INSTITUCION',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (6,N'Código Penal',N'INSTRUMENTO',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (6,N'Estado Plurinacional de Bolivia',N'LOC',1);
INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES (6,N'Dr. Ramiro Castillo Vega',N'PER',1);