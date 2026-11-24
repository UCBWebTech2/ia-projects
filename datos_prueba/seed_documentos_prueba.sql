SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM dbo.Documentos WHERE nombreDocumento=N'CIVIL_01_Demanda_Cumplimiento_Contrato')
BEGIN
  INSERT INTO dbo.Documentos (nombreDocumento,contenidoDocumento,rutaDocumento,tipoDocumento,fechaCreacion,fechaModificacion,idUsuario,idProceso,idCategoria,habilitado)
  VALUES (N'CIVIL_01_Demanda_Cumplimiento_Contrato',N'ESTUDIO JUR?DICO Z??IGA
Juzgado P?blico Civil y Comercial - Santa Cruz de la Sierra
DEMANDA ORDINARIA DE CUMPLIMIENTO DE CONTRATO
______________________________________________________________________________
SE?OR JUEZ P?BLICO EN LO CIVIL Y COMERCIAL:
I. DATOS DE LAS PARTES
DEMANDANTE: Carlos Alberto M?ndez Rojas, con C.I. 3456789 SC, mayor de edad, h?bil por derecho, domiciliado en la Av. Banzer N? 450, Santa Cruz de la Sierra.
DEMANDADO: Constructora Altiplano S.R.L., representada legalmente por su Gerente General Ing. Roberto Su?rez Pe?a, con domicilio legal en Calle Warnes N? 120, Santa Cruz de la Sierra.
II. OBJETO DE LA DEMANDA
El demandante interpone demanda ordinaria de cumplimiento de contrato, m?s da?os y perjuicios, contra Constructora Altiplano S.R.L., por incumplimiento del Contrato de Construcci?n suscrito en fecha 15 de marzo de 2025.
III. HECHOS
1. En fecha 15 de marzo de 2025, las partes suscribieron un Contrato de Construcci?n para la edificaci?n de una vivienda unifamiliar en el Barrio Equipetrol, por el monto total de Bs. 450.000.- (Cuatrocientos Cincuenta Mil 00/100 Bolivianos).
2. El plazo de entrega fue pactado para el 15 de septiembre de 2025, habiendo transcurrido m?s de seis meses sin que la obra haya sido concluida.
3. A pesar de los requerimientos escritos enviados en fechas 20 de octubre de 2025 y 10 de noviembre de 2025, la parte demandada no cumpli? con sus obligaciones contractuales.
IV. FUNDAMENTOS DE DERECHO
Se funda la presente demanda en los art?culos 519, 568 y 574 del C?digo Civil boliviano, que establecen la obligaci?n de cumplir los contratos y la responsabilidad por da?os y perjuicios derivados del incumplimiento.
V. PETITORIO
Solicito a su autoridad: 1) Admitir la demanda; 2) Ordenar el cumplimiento del contrato o la devoluci?n de los montos pagados; 3) Condenar al pago de da?os y perjuicios; 4) Imponer costas y costos.
Santa Cruz de la Sierra, 10 de abril de 2026.
Abg. Mar?a Fernanda Z??iga
Matr?cula C.A.S.C. 9876','Documentos_Proyecto/CIVIL_01_Demanda_Cumplimiento_Contrato.docx','Demanda',GETDATE(),GETDATE(),1,1,1,1);
END


IF NOT EXISTS (SELECT 1 FROM dbo.Documentos WHERE nombreDocumento=N'CIVIL_02_Contestacion_Demanda')
BEGIN
  INSERT INTO dbo.Documentos (nombreDocumento,contenidoDocumento,rutaDocumento,tipoDocumento,fechaCreacion,fechaModificacion,idUsuario,idProceso,idCategoria,habilitado)
  VALUES (N'CIVIL_02_Contestacion_Demanda',N'ESTUDIO JUR?DICO Z??IGA
Juzgado P?blico Civil y Comercial - Santa Cruz de la Sierra
CONTESTACI?N A DEMANDA ORDINARIA
______________________________________________________________________________
SE?OR JUEZ P?BLICO EN LO CIVIL Y COMERCIAL:
Roberto Su?rez Pe?a, en representaci?n legal de Constructora Altiplano S.R.L., dentro del proceso ordinario de cumplimiento de contrato seguido por Carlos Alberto M?ndez Rojas, responde a la demanda en los siguientes t?rminos:
I. NEGATIVA GENERAL Y ESPEC?FICA
Niego de forma general y espec?fica todos los hechos expuestos en la demanda, salvo aquellos que sean expresamente reconocidos en la presente contestaci?n.
II. HECHOS
1. Es cierto que se suscribi? el Contrato de Construcci?n en fecha 15 de marzo de 2025; sin embargo, el demandante incumpli? con los pagos parciales acordados en las cl?usulas cuarta y quinta.
2. La demora en la ejecuci?n de la obra fue consecuencia directa de la falta de pago oportuno por parte del demandante, lo cual constituye excepci?n de contrato no cumplido.
III. FUNDAMENTOS DE DERECHO
Invoco los art?culos 568 y 569 del C?digo Civil, que establecen la excepci?n de contrato no cumplido, as? como el principio de buena fe contractual.
IV. PETITORIO
Solicito: 1) Rechazar la demanda por improbada; 2) Declarar la excepci?n de contrato no cumplido; 3) Imponer costas al demandante.
Santa Cruz de la Sierra, 25 de abril de 2026.
Abg. Jorge Luis Camacho
Matr?cula C.A.S.C. 5432','Documentos_Proyecto/CIVIL_02_Contestacion_Demanda.docx','Contestacion',GETDATE(),GETDATE(),1,1,1,1);
END


IF NOT EXISTS (SELECT 1 FROM dbo.Documentos WHERE nombreDocumento=N'CIVIL_03_Sentencia_Cumplimiento')
BEGIN
  INSERT INTO dbo.Documentos (nombreDocumento,contenidoDocumento,rutaDocumento,tipoDocumento,fechaCreacion,fechaModificacion,idUsuario,idProceso,idCategoria,habilitado)
  VALUES (N'CIVIL_03_Sentencia_Cumplimiento',N'ESTUDIO JUR?DICO Z??IGA
Juzgado P?blico Civil y Comercial N? 3 - Santa Cruz de la Sierra
SENTENCIA CIVIL N? 045/2026
______________________________________________________________________________
PROCESO: Ordinario de Cumplimiento de Contrato
DEMANDANTE: Carlos Alberto M?ndez Rojas
DEMANDADO: Constructora Altiplano S.R.L.
VISTOS: La demanda, contestaci?n, pruebas ofrecidas y producidas, y todo cuanto de autos consta;
CONSIDERANDO:
I. Que, de la revisi?n de los antecedentes procesales, se evidencia la existencia del Contrato de Construcci?n suscrito el 15 de marzo de 2025, documento que no fue objetado por la parte demandada.
II. Que, la parte demandante ha demostrado el incumplimiento del plazo pactado, mientras que la parte demandada no acredit? de forma suficiente la excepci?n de contrato no cumplido.
III. Que, conforme a los art?culos 519 y 574 del C?digo Civil, corresponde disponer el cumplimiento de la obligaci?n contractual y la reparaci?n de da?os probados.
POR TANTO:
La Juez P?blica en lo Civil y Comercial N? 3, administrando justicia en nombre del Estado Plurinacional de Bolivia, FALLA: Declarando PROBADA en parte la demanda ordinaria interpuesta por Carlos Alberto M?ndez Rojas contra Constructora Altiplano S.R.L., disponiendo el cumplimiento del contrato en el plazo de 60 d?as calendario, bajo conminatoria de ejecuci?n forzosa.
Reg?strese, notif?quese y c?mplase.
Dra. Patricia R?os Mendoza
Jueza P?blica Civil y Comercial N? 3','Documentos_Proyecto/CIVIL_03_Sentencia_Cumplimiento.docx','Sentencia',GETDATE(),GETDATE(),1,1,1,1);
END


IF NOT EXISTS (SELECT 1 FROM dbo.Documentos WHERE nombreDocumento=N'PENAL_01_Denuncia_Estafa')
BEGIN
  INSERT INTO dbo.Documentos (nombreDocumento,contenidoDocumento,rutaDocumento,tipoDocumento,fechaCreacion,fechaModificacion,idUsuario,idProceso,idCategoria,habilitado)
  VALUES (N'PENAL_01_Denuncia_Estafa',N'ESTUDIO JUR?DICO Z??IGA
Ministerio P?blico - Fiscal?a Departamental de La Paz
DENUNCIA PENAL POR ESTAFA
______________________________________________________________________________
SE?OR FISCAL DE MATERIA:
I. DATOS DEL DENUNCIANTE
Ana Luc?a Vargas Torrez, con C.I. 6123456 LP, mayor de edad, domiciliada en Calle Murillo N? 789, La Paz, interpone formal denuncia penal contra Juan Carlos Quispe Mamani por la presunta comisi?n del delito de Estafa.
II. DATOS DEL DENUNCIADO
Juan Carlos Quispe Mamani, con C.I. 4987654 LP, domiciliado en Zona Villa F?tima, Calle 2 N? 100, La Paz.
III. HECHOS
1. En fecha 5 de enero de 2026, el denunciado se present? como representante de una empresa de inversiones y ofreci? a la denunciante un rendimiento del 20% mensual.
2. La denunciante entreg? la suma de Bs. 80.000.- (Ochenta Mil 00/100 Bolivianos) mediante transferencia bancaria y efectivo, seg?n comprobantes adjuntos.
3. Desde el 20 de febrero de 2026, el denunciado dej? de responder llamadas y se constat? que la oficina indicada no existe.
IV. TIPIFICACI?N
Los hechos descritos se adecuan al tipo penal de Estafa previsto en el art?culo 335 del C?digo Penal boliviano, al haber inducido a error a la v?ctima para obtener un beneficio econ?mico indebido.
V. PETITORIO
Solicito: 1) Recepci?n de la denuncia; 2) Inicio de investigaci?n penal; 3) Citaci?n del denunciado; 4) Secuestro de documentaci?n bancaria; 5) Cautela de bienes del denunciado.
La Paz, 15 de marzo de 2026.
Ana Luc?a Vargas Torrez
C.I. 6123456 LP','Documentos_Proyecto/PENAL_01_Denuncia_Estafa.docx','Denuncia',GETDATE(),GETDATE(),1,2,2,1);
END


IF NOT EXISTS (SELECT 1 FROM dbo.Documentos WHERE nombreDocumento=N'PENAL_02_Querella_Lesiones_Graves')
BEGIN
  INSERT INTO dbo.Documentos (nombreDocumento,contenidoDocumento,rutaDocumento,tipoDocumento,fechaCreacion,fechaModificacion,idUsuario,idProceso,idCategoria,habilitado)
  VALUES (N'PENAL_02_Querella_Lesiones_Graves',N'ESTUDIO JUR?DICO Z??IGA
Juzgado de Instrucci?n en lo Penal - Cochabamba
QUERELLA PENAL POR LESIONES GRAVES
______________________________________________________________________________
SE?OR JUEZ DE INSTRUCCI?N EN LO PENAL:
I. DATOS DEL QUERELLANTE
Miguel ?ngel Flores Guti?rrez, con C.I. 5234567 CB, domiciliado en Av. Am?rica N? 321, Cochabamba.
II. DATOS DEL QUERELLADO
Pedro Luis Mamani Condori, con C.I. 7345678 CB, con domicilio conocido en Zona Sud, Cochabamba.
III. RELACI?N DE LOS HECHOS
El d?a 2 de febrero de 2026, aproximadamente a horas 22:30, en las inmediaciones de la Av. Ayacucho, el querellado agredi? f?sicamente al querellante con un objeto contundente, ocasion?ndole lesiones de consideraci?n.
El Certificado M?dico Forense N? 123/2026 establece una incapacidad de 25 d?as, calificando las lesiones como graves conforme a la normativa penal vigente.
IV. FUNDAMENTOS JUR?DICOS
La conducta se subsume en el delito de Lesiones Graves previsto en el art?culo 271 del C?digo Penal boliviano. Se solicita la aplicaci?n de medidas cautelares conforme al art?culo 231 del C?digo Procesal Penal.
V. PETITORIO
Solicito: 1) Admisi?n de la querella; 2) Imputaci?n formal; 3) Imposici?n de medidas cautelares; 4) Reparaci?n del da?o civil.
Cochabamba, 20 de febrero de 2026.
Abg. Laura Patricia Monta?o
Matr?cula C.A.C. 3344','Documentos_Proyecto/PENAL_02_Querella_Lesiones_Graves.docx','Querella',GETDATE(),GETDATE(),1,3,2,1);
END


IF NOT EXISTS (SELECT 1 FROM dbo.Documentos WHERE nombreDocumento=N'PENAL_03_Sentencia_Estafa')
BEGIN
  INSERT INTO dbo.Documentos (nombreDocumento,contenidoDocumento,rutaDocumento,tipoDocumento,fechaCreacion,fechaModificacion,idUsuario,idProceso,idCategoria,habilitado)
  VALUES (N'PENAL_03_Sentencia_Estafa',N'ESTUDIO JUR?DICO Z??IGA
Tribunal de Sentencia Penal N? 1 - La Paz
SENTENCIA PENAL N? 112/2026
______________________________________________________________________________
PROCESO: Penal p?blico por el delito de Estafa
ACUSADO: Juan Carlos Quispe Mamani
V?CTIMA: Ana Luc?a Vargas Torrez
VISTOS Y CONSIDERANDO:
Que, durante el juicio oral, p?blico y contradictorio, se produjo la prueba testimonial, documental y pericial ofrecida por el Ministerio P?blico y la v?ctima, acredit?ndose que el acusado obtuvo la suma de Bs. 80.000.- mediante enga?o.
Que, la defensa no logr? desvirtuar los elementos constitutivos del tipo penal previsto en el art?culo 335 del C?digo Penal.
POR TANTO:
El Tribunal de Sentencia Penal N? 1, administrando justicia en nombre del Estado Plurinacional de Bolivia, declara a Juan Carlos Quispe Mamani AUTOR y CULPABLE del delito de Estafa, imponiendo la pena de cinco (5) a?os de presidio, m?s el pago de responsabilidad civil a favor de la v?ctima.
Reg?strese, notif?quese y ejec?tese conforme a ley.
Dr. Ramiro Castillo Vega
Presidente del Tribunal de Sentencia Penal N? 1','Documentos_Proyecto/PENAL_03_Sentencia_Estafa.docx','Sentencia',GETDATE(),GETDATE(),1,2,2,1);
END
