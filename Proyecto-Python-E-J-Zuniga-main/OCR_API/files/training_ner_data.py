TRAIN_DATA = [
    ("La abogada María Pérez presentó la demanda en La Paz contra la empresa XYZ S.A.", 
     {"entities": [(13, 24, "PER"), (48, 54, "LOC"), (65, 73, "ORG")]}),
    ("El artículo 34 de la Ley 1970 establece los derechos del imputado.",
     {"entities": [(3, 13, "ARTICULO"), (21, 30, "LEY")]}),
    ("La Fiscalía de Cochabamba remitió el expediente al Tribunal Supremo de Justicia.",
     {"entities": [(13, 25, "LOC"), (53, 83, "ORG")]}),
    ("EMBOTELLADORAS BOLIVIANAS UNIDAS S.A. y la Sra. Oriana Marioly Hoyos Balbontin celebran el presente contrato", 
     {"entities": [(0, 38, "ORG"), (46, 78, "PERSON")]}),
    ("El Sr. Orlando Piro Borquez con C.I. 5339726 SC representa a la empresa", 
     {"entities": [(7, 29, "PERSON")]}),
    ("La empresa EMBOTELLADORAS BOLIVIANAS UNIDAS S.A. tiene su casa matriz en Santa Cruz de la Sierra", {
        "entities": [(12, 50, "ORG"), (72, 96, "LOC")]}),
    ("La Sra. Oriana Marioly Hoyos Balbontin con C.I. 3898884 S.C. trabajará como Secretaria", {
        "entities": [(8, 40, "PERSON")]}),
    ("El contrato fue firmado en la ciudad de Santa Cruz de la Sierra el 9 de enero", {
        "entities": [(31, 55, "LOC")]}),
    ("P/EMBOL S.A. Orlando Piro Bórquez GERENTE GENERAL", {
        "entities": [(2, 12, "ORG"), (14, 33, "PERSON")]}),
    ("Luis Enrique Gil Landivar firmó el contrato como trabajador", {
        "entities": [(0, 28, "PERSON")]}),
    ("ABDON CONDORI APANA, ALDO CHAMBI SILVA Y ROSAURA SEVILLANOS NN…., mayores de edad, hábiles por derecho, naturales y domiciliados en el Municipio de Apolo, Primera Sección de la Prov. Franz Tamayo del Departamento de La Paz",
        {"entities": [
            (0, 20, "PERSON"),
            (22, 39, "PERSON"),
            (42, 63, "PERSON"),
            (115, 133, "LOC"),
            (136, 154, "LOC"),
            (162, 180, "LOC")]}),
    ("seguido por nuestro detractor DIONISIO GUTIERREZ, por presuntos e imaginarios delitos, a cargo del entonces Fiscal Dr. FACUNDO CORONEL CHOQUE",
        {"entities": [
            (33, 51, "PERSON"),
            (92, 116, "PERSON")
        ]}
    ),
    ("hacemos conocer a vuestra autoridad que nuestro patrocinante al presente es el Abog. Antonio Rocabado Reynal con RPA1548694-ARR",
        {"entities": [(93, 117, "PERSON"), (123, 139, "ORG")]}),
    ("Calle Miguel de Urrea Esq. Calle Florida del Barrio Iltasagua, Municipio de Apolo, Prov. Franz Tamayo del Departamento de La Paz",
        {"entities": [
            (6, 24, "LOC"),
            (30, 43, "LOC"),
            (49, 67, "LOC"),
            (70, 86, "LOC"),
            (89, 107, "LOC"),
            (112, 130, "LOC")
        ]}
    ),
    ("Ramiro Cuevas Echave apoderado legal de Katty Odeth Cuevas Laura",
        {"entities": [
            (0, 20, "PERSON"),
            (45, 71, "PERSON")
        ]}
    ),
    ("por la funcionaria de su Despacho señora GLADYS FLOR MIRANDA LAZO",
        {"entities": [
            (44, 72, "PERSON")
        ]}
    ),
    ("en presencia de mi persona (Mirtha Sánchez Zarate), con los actuados del caso",
        {"entities": [(30, 52, "PERSON")]}),
    ("SEÑOR REPRESENTANTE DEL MINISTERIO PÚBLICO ADSCRITO A APOLO PROV. FRANZ TAMAYO (Dr. Limbert M. Orozco Carvajal)", 
     {"entities": [(46, 69, "LOC"), (72, 102, "PER")]}),
    
    ("MIRTHA SANCHEZ ZARATE y KELLY CLAUDIA MOLLINEDO SANCHEZ", 
     {"entities": [(0, 23, "PER"), (26, 58, "PER")]}),
    
    ("Ramiro Cuevas Echave, Milca Lea Sarmiento Carrión, Nelson Oblitas Quispe y Otros", 
     {"entities": [(0, 21, "PER"), (23, 51, "PER"), (53, 74, "PER")]}),
    
    ("Av. Centenario esq. Miguel de Urrea y Florida del Barrio Iltasagua de Apolo, Prov. Franz Tamayo", 
     {"entities": [(0, 75, "LOC")]}),
    
    ("Adrián Mollinedo Montaño", 
     {"entities": [(0, 25, "PER")]}),
    
    ("Cristina Daleney Vda. de Silva", 
     {"entities": [(0, 30, "PER")]}),
    
    ("Notaria de Fe Púbica de apolo a cargo de la señora Claudia M. Gariazu Gámez", 
     {"entities": [(27, 32, "LOC"), (48, 74, "PER")]}),
    
    ("Katty Odeth Cuevas Laura", 
     {"entities": [(0, 25, "PER")]}),
    
    ("Barrio Iltasagua de Apolo", 
     {"entities": [(0, 27, "LOC")]}),
    
    ("Dr. Ovidio Duran", 
     {"entities": [(0, 16, "PER")]}),
    
    ("Mirtha Sánchez Zarate", 
     {"entities": [(0, 22, "PER")]}),
    
    ("Zenón Flores Mamani con C.I. No. 2359913 LP., domiciliado en la comunidad Yalihuara de Apolo", 
     {"entities": [(0, 21, "PER"), (67, 91, "LOC")]}),
    
    ("Virginia Roxana Apana, con C.I. No. 10100716 LP., domiciliada en la localidad de Mapiri", 
     {"entities": [(0, 22, "PER"), (69, 75, "LOC")]}),
    
    ("Zenón Benedicto Aguirre Avelino, con C.I. No. 2400265-1C LP., domiciliado en el Barrio Iltasagua de Apolo", 
     {"entities": [(0, 33, "PER"), (73, 100, "LOC")]}),
]