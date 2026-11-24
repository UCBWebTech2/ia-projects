import unicodedata
import re
import spacy
from spacy.pipeline import EntityRuler


# Cargar el modelo en español
nlp = spacy.load("es_core_news_lg")

# Añadir EntityRuler al pipeline antes del NER
# Phase 3 (AL-02): avoid overwriting statistical NER entities by default.
ruler = nlp.add_pipe("entity_ruler", before="ner", config={"overwrite_ents": False})

# Patrones para detectar LEY, ARTICULO e INSTRUMENTO, incluyendo "Arts."
patterns = [
    # Patrones para LEY
    {"label": "LEY", "pattern": [{"LOWER": "ley"}, {"LOWER": {"IN": ["no", "número", "nº", "num", "n"]}, "OP": "?"}, {"IS_PUNCT": True, "OP": "?"}, {"IS_DIGIT": True}]},
    
    # Patrones de INSTRUMENTO
    {"label": "INSTRUMENTO", "pattern": "Convenio 169"},
    {"label": "INSTRUMENTO", "pattern": [{"LOWER": "declaración"}, {"LOWER": "de"}, {"LOWER": "la"}, {"LOWER": "onu"}]},
    {"label": "INSTRUMENTO", "pattern": [{"LOWER": "código"}, {"LOWER": "penal"}]},
    {"label": "INSTRUMENTO", "pattern": [{"LOWER": "código"}, {"LOWER": "de"}, {"LOWER": "procedimiento"}, {"LOWER": "penal"}]},
    {"label": "INSTRUMENTO", "pattern": [{"LOWER": "constitución"}, {"LOWER": "política"}, {"LOWER": "del"}, {"LOWER": "estado"}]},

    # Patrones de INSTITUCION jurídica
    {"label": "INSTITUCION", "pattern": [
        {"LOWER": "ministerio"},
        {"LOWER": {"IN": ["público", "publico"]}}
    ]},

    {"label": "INSTITUCION", "pattern": [
        {"LOWER": {"IN": ["fiscalía", "fiscalia"]}},
        {"LOWER": "departamental", "OP": "?"},
        {"LOWER": "de", "OP": "?"},
        {"LOWER": {"IN": ["la", "el"]}, "OP": "?"},
        {"LOWER": {"IN": ["paz", "alto", "oruro", "potosí", "potosi", "tarija", "beni", "pando", "chuquisaca"]}, "OP": "?"}
    ]},

    {"label": "INSTITUCION", "pattern": [
        {"LOWER": "fiscalía"},
        {"LOWER": "de"},
        {"LOWER": "materia"}
    ]},

    {"label": "INSTITUCION", "pattern": [
        {"LOWER": "fiscalia"},
        {"LOWER": "de"},
        {"LOWER": "materia"}
    ]},

    {"label": "INSTITUCION", "pattern": [
        {"LOWER": "juzgado"},
        {"LOWER": {"IN": ["público", "publico", "de"]}, "OP": "*"},
        {"LOWER": {"IN": ["civil", "penal", "familiar", "familia", "comercial", "instrucción", "instruccion", "sentencia", "partido", "garantías", "garantias", "anticorrupción", "anticorrupcion"]}, "OP": "+"},
        {"LOWER": {"IN": ["y", "en", "lo", "de"]}, "OP": "*"},
        {"LOWER": {"IN": ["civil", "penal", "familiar", "familia", "comercial", "instrucción", "instruccion"]}, "OP": "*"}
    ]},
    {"label": "INSTITUCION", "pattern": [
        {"LOWER": "tribunal"},
        {"LOWER": "de"},
        {"LOWER": "sentencia"},
        {"LOWER": {"IN": ["penal", "civil"]}, "OP": "?"}
    ]},
    {"label": "INSTITUCION", "pattern": [
        {"LOWER": "ministerio"},
        {"LOWER": {"IN": ["público", "publico"]}}
    ]},
    {"label": "INSTITUCION", "pattern": [
        {"LOWER": {"IN": ["fiscalía", "fiscalia"]}},
        {"LOWER": "departamental", "OP": "?"}
    ]},

    {"label": "INSTITUCION", "pattern": [
        {"LOWER": "tribunal"},
        {"LOWER": {"IN": ["supremo", "departamental"]}},
        {"LOWER": "de"},
        {"LOWER": "justicia"}
    ]},

    {"label": "INSTITUCION", "pattern": [
        {"LOWER": "consejo"},
        {"LOWER": "de"},
        {"LOWER": "la"},
        {"LOWER": "magistratura"}
    ]},

    {"label": "INSTITUCION", "pattern": [
        {"LOWER": "derechos"},
        {"LOWER": "reales"}
    ]},

    {"label": "INSTITUCION", "pattern": [
        {"LOWER": {"IN": ["notaría", "notaria"]}},
        {"LOWER": "de"},
        {"LOWER": "fe"},
        {"LOWER": {"IN": ["pública", "publica"]}}
    ]},

    {"label": "INSTITUCION", "pattern": [
        {"LOWER": "servicio"},
        {"LOWER": "de"},
        {"LOWER": "registro"},
        {"LOWER": {"IN": ["cívico", "civico"]}}
    ]},

    {"label": "INSTITUCION", "pattern": [
        {"LOWER": "policía"},
        {"LOWER": "boliviana"}
    ]},

    {"label": "INSTITUCION", "pattern": [
        {"LOWER": "policia"},
        {"LOWER": "boliviana"}
    ]},

    {"label": "INSTITUCION", "pattern": "FELCC"},
    {"label": "INSTITUCION", "pattern": "FELCV"},
    # Identificadores procesales
    {"label": "INSTRUMENTO", "pattern": [
        {"LOWER": {"IN": ["nurej", "expediente", "exp.", "exp"]}},
        {"TEXT": {"REGEX": r"^[:#N°º\-]*$"}, "OP": "?"},
        {"TEXT": {"REGEX": r"^[A-Za-z0-9\-\/\.]+$"}, "OP": "+"}
    ]},
    {"label": "INSTRUMENTO", "pattern": [
        {"LOWER": {"IN": ["proceso", "causa"]}},
        {"LOWER": {"IN": ["n°", "nº", "no", "numero", "número"]}},
        {"TEXT": {"REGEX": r"^[0-9A-Za-z\-\/\.]+$"}, "OP": "+"}
    ]},
]
assert isinstance(ruler, EntityRuler), "El componente 'entity_ruler' no se ha añadido correctamente al pipeline."
# Agregar patrones al Entity Ruler
ruler.add_patterns(patterns)

# Lista de términos a excluir para evitar falsos positivos
exclusion_terms = {
    "OTROSÍ", "HECHOS", "DEUDOR", "DIGNIDAD", "CONSISTE", "Nurej", 
    "POSIBILIDAD", "CIERTO", "CALUMNIAS", "INJURIAS", "OTROSI.-", "Teléfono",
    "Justicia", "CONSTITUCIONALIDAD", "VALOR INDIVIDUAL", "COMO",
    "DAÑOS", "DANOS", "DAÑO", "DANO", "OBLIGACIÓN", "OBLIGACION",
    "SOLICITO", "SOLICITA", "DEMANDA", "CONTESTACIÓN", "CONTESTACION",
    "SENTENCIA", "DENUNCIA", "QUERELLA", "PETITORIO", "VISTOS", "CONSIDERANDO",
    "POR TANTO", "REGÍSTRESE", "REGISTRESE", "NOTIFÍQUESE", "NOTIFIQUESE",
    "CÚMPLASE", "CUMPLASE", "EXPONE", "FUNDAMENTOS", "PRUEBAS",
    "FUNDAMENTOS JURÍDICOS", "FUNDAMENTOS JURIDICOS", "FUNDAMENTOS DE DERECHO",
    "RELACIÓN DE LOS HECHOS", "RELACION DE LOS HECHOS",
    "INVOCO", "NIEGO", "CONTESTACIÓN A", "CONTESTACION A", "COMERCIAL N°", "COMERCIAL N",
}
ALLOWED_LABELS = {"LEY", "ARTICULO", "INSTRUMENTO", "PER", "LOC", "ORG"}
PALABRAS_RUIDO_GENERALES = {
    "ART", "ARTS", "ART.", "ARTS.",
    "RESOLUCION", "RESOLUCIÓN",
    "DELITOS",
    "CONCLUSIVO",
    "EXPONE",
    "ANTECEDENTES",
    "APELLIDOS",
    "PAR",
    "FIN",
    "INFORME",
    "FORMULAR",
    "NOTIFÍQUESE", "NOTIFIQUESE"
}
ETIQUETAS_JURIDICAS = {
    "LEY",
    "ARTICULO",
    "INSTRUMENTO",
    "INSTITUCION",
    "DEMANDANTE",
    "DEMANDADO",
    "JUEZ",
    "FISCAL",
    "ABOGADO",
    "NRO_PROCESO",
    "JUZGADO",
    "FECHA_AUDIENCIA",
}

ETIQUETAS_GENERICAS = {
    "PER",
    "LOC",
    "ORG"
}
ARTICLE_LIST_RE = re.compile(
    r"""
    \b
    (?:art[íi]culos?|arts?\.?)
    \s*
    (?:
        \d+[°º]?
        (?:\s*\([^)]*\))?
        (?:
            \s*
            (?:núm|num|inc|incs|inciso|par|parágrafo|paragrafo)\.?
            \s*
            \d+[)]?
            (?:\s*(?:,|y)\s*\d+[)]?)*
        )?
    )
    (?:
        \s*(?:,|y)\s*
        (?:
            \d+[°º]?
            (?:\s*\([^)]*\))?
            (?:
                \s*
                (?:núm|num|inc|incs|inciso|par|parágrafo|paragrafo)\.?
                \s*
                \d+[)]?
                (?:\s*(?:,|y)\s*\d+[)]?)*
            )?
        )
    )*
    """,
    re.IGNORECASE | re.VERBOSE
)
def quitar_acentos(texto: str) -> str:
    texto_normalizado = unicodedata.normalize("NFD", texto)
    return "".join(
        caracter for caracter in texto_normalizado
        if unicodedata.category(caracter) != "Mn"
    )


def normalizar_base_para_clave(texto: str) -> str:
    texto = normalizar_entidad(texto)
    texto = texto.lower()
    texto = quitar_acentos(texto)

    # Quitar comillas y caracteres decorativos
    texto = re.sub(r"[“”\"'`´]", "", texto)

    # Normalizar espacios
    texto = re.sub(r"\s+", " ", texto)

    # Quitar signos sobrantes al inicio/final
    texto = texto.strip(" .,:;-/")

    return texto
def normalizar_clave_articulo(texto: str) -> str:
    texto = normalizar_base_para_clave(texto)

    # Unificar art, art., arts, arts., artículo, artículos
    texto = re.sub(r"\barticulos?\b|\barts?\b\.?", "art", texto)

    # Unificar núm, num, número, nro
    texto = re.sub(r"\bnum\.?\b|\bnumero\b|\bnro\.?\b|\bnº\b", "num", texto)

    # Unificar inc, incs, inciso, incisos
    texto = re.sub(r"\bincs?\.?\b|\bincisos?\b", "inc", texto)

    # Unificar par, parágrafo, paragrafo
    texto = re.sub(r"\bpar\.?\b|\bparagrafos?\b", "par", texto)

    # Quitar paréntesis sobrantes
    texto = re.sub(r"[()]", "", texto)

    # Quitar puntos sobrantes
    texto = texto.replace(".", "")

    # Normalizar coma y espacios
    texto = re.sub(r"\s*,\s*", ",", texto)
    texto = re.sub(r"\b(inc|num|par)\s*(\d+)", r"\1 \2", texto)
    texto = re.sub(r"\s+", " ", texto).strip()

    return texto

def normalizar_clave_entidad(texto: str, label: str) -> tuple[str, str]:
    if label == "ARTICULO":
        clave_texto = normalizar_clave_articulo(texto)
    else:
        clave_texto = normalizar_base_para_clave(texto)

        # Opcional para personas: quitar títulos comunes
        if label == "PER":
            clave_texto = re.sub(
                r"\b(dr|dra|abg|abogado|lic|sr|sra)\b\.?\s*",
                "",
                clave_texto
            )
            clave_texto = re.sub(r"\s+", " ", clave_texto).strip()

    return (label, clave_texto)


def elegir_texto_representativo(actual: str, nuevo: str) -> str:
    """
    Elige qué texto mostrar al usuario cuando dos entidades son equivalentes.
    Regla simple:
    - preferir el que no esté todo en mayúsculas
    - preferir el más largo si parece más completo
    """
    if actual.isupper() and not nuevo.isupper():
        return nuevo

    if len(nuevo) > len(actual) and len(nuevo) <= 120:
        return nuevo

    return actual


LABEL_PRIORITY = {
    "LEY": 7,
    "ARTICULO": 7,
    "INSTRUMENTO": 7,
    "NRO_PROCESO": 7,
    "FECHA_AUDIENCIA": 7,
    "INSTITUCION": 6,
    "JUZGADO": 6,
    "DEMANDANTE": 6,
    "DEMANDADO": 6,
    "JUEZ": 6,
    "FISCAL": 6,
    "ABOGADO": 6,
    "PER": 5,
    "LOC": 4,
    "ORG": 3,
}


def _etiquetas_compatibles(a: str, b: str) -> bool:
    if a == b:
        return True
    pares = [
        {"ORG", "INSTITUCION"},
        {"INSTITUCION", "JUZGADO"},
        {"PER", "DEMANDANTE"},
        {"PER", "DEMANDADO"},
        {"PER", "JUEZ"},
        {"PER", "FISCAL"},
        {"PER", "ABOGADO"},
        {"INSTRUMENTO", "NRO_PROCESO"},
    ]
    s = {a, b}
    return any(s <= p for p in pares)


def eliminar_duplicados_semanticos(entities, incluir_frecuencia=False):
    acumuladas = {}

    for entity in entities:
        texto = entity.get("text") or entity.get("Termino") or ""
        label = entity.get("label") or entity.get("TipoTermino") or ""

        texto = normalizar_entidad(texto)

        if not texto or not label:
            continue

        # Corrección heurística: honorífico + nombre => PER
        if label in {"LOC", "ORG"} and re.match(r"^(Dr\.?|Dra\.?|Abg\.?|Abogado|Abogada)\b", texto, re.IGNORECASE):
            if len(texto.split()) >= 2:
                label = "PER"

        if not es_entidad_valida(texto, label):
            continue

        _, clave_texto = normalizar_clave_entidad(texto, label)
        frecuencia = entity.get("Frecuencia", 1)

        clave_existente = None
        if (label, clave_texto) in acumuladas:
            clave_existente = (label, clave_texto)
        else:
            for k in acumuladas:
                if k[1] == clave_texto and _etiquetas_compatibles(k[0], label):
                    clave_existente = k
                    break

        if clave_existente is None:
            acumuladas[(label, clave_texto)] = {
                "text": texto,
                "label": label,
                "frequency": frecuencia
            }
        else:
            actual = acumuladas[clave_existente]
            actual["frequency"] += frecuencia
            actual["text"] = elegir_texto_representativo(actual["text"], texto)
            # promover etiqueta de mayor prioridad (p.ej. ORG -> INSTITUCION)
            if LABEL_PRIORITY.get(label, 0) > LABEL_PRIORITY.get(actual["label"], 0):
                nuevo = {"text": actual["text"], "label": label, "frequency": actual["frequency"]}
                del acumuladas[clave_existente]
                acumuladas[(label, clave_texto)] = nuevo

    resultado = []

    for entidad in acumuladas.values():
        item = {
            "text": entidad["text"],
            "label": entidad["label"]
        }

        if incluir_frecuencia:
            item["frequency"] = entidad["frequency"]

        resultado.append(item)

    return resultado

def es_entidad_valida(texto:str, label:str) -> bool:
    texto_limpio = normalizar_entidad(texto)
    texto_mayus = texto_limpio.upper()

    if not texto_limpio:
        return False

    etiquetas_validas = ETIQUETAS_JURIDICAS | ETIQUETAS_GENERICAS

    if label not in etiquetas_validas:
        return False

    exclusion_terms_mayus = {term.upper() for term in exclusion_terms}

    if texto_mayus in exclusion_terms_mayus:
        return False

    if texto_mayus in PALABRAS_RUIDO_GENERALES:
        return False

    if texto_limpio.isdigit():
        return False

    # IDs/documentos no son entidades nombradas útiles
    if re.match(r"^(C\.?\s*I\.?|CI|NIT|NUREJ|EXP\.?|MATR[ÍI]CULA|CASE?)[\s:.\-N°º0-9A-Z/]+$", texto_mayus):
        return False
    if label in {"PER", "ORG"} and re.search(r"\b(C\.?\s*I\.?|NIT|NUREJ)\b", texto_mayus):
        return False

    # Roles procesales/títulos y encabezados no son entidades nombradas útiles
    if label in {"PER", "JUEZ", "FISCAL", "ABOGADO", "DEMANDANTE", "DEMANDADO"}:
        roles_solo = {
            "JUEZ", "JUEZA", "FISCAL", "SEÑOR", "SEÑORA", "DEMANDANTE", "DEMANDADO",
            "QUERELLANTE", "QUERELLADO", "IMPUTADO", "ACUSADO", "VICTIMA", "VÍCTIMA",
            "ABOGADO", "ABOGADA", "MAGISTRADO", "MAGISTRADA", "SECRETARIO", "SECRETARIA",
            "PRESIDENTE", "VOCAL", "PERITO", "DR", "DRA", "ABG",
        }
        conectores_rol = {
            "PUBLICO", "PÚBLICO", "PUBLICA", "PÚBLICA", "EN", "LO", "DE", "LA", "EL", "DEL", "MATERIA",
            "CIVIL", "PENAL", "Y", "COMERCIAL", "FAMILIAR", "FAMILIA", "INSTRUCCION",
            "INSTRUCCIÓN", "SENTENCIA", "PARTIDO", "GARANTIAS", "GARANTÍAS",
            "ANTICORRUPCION", "ANTICORRUPCIÓN", "TRIBUNAL", "MIXTO", "PRIMERO",
            "SEGUNDO", "TERCERO", "N", "NO", "PRIMERA", "SEGUNDA", "TERCERA",
        }
        tokens = [re.sub(r"[^A-ZÁÉÍÓÚÑ0-9]", "", t) for t in re.split(r"\s+", texto_mayus) if t]
        if tokens and all(t in roles_solo | conectores_rol or t.isdigit() for t in tokens):
            return False
        encabezados = {
            "FUNDAMENTOS", "JURIDICOS", "JURÍDICOS", "DERECHO", "HECHOS",
            "ANTECEDENTES", "PETITORIO", "PRUEBAS", "CONSIDERANDO", "VISTOS",
            "POR", "TANTO", "OBJETO", "RELACION", "RELACIÓN",
        }
        if tokens and all(t in encabezados for t in tokens):
            return False

    if label in {"PER", "LOC", "ORG"} and len(texto_limpio) <= 2:
        return False

    letras = sum(1 for c in texto_limpio if c.isalpha())

    if label in {"PER", "LOC", "ORG"} and letras < 3:
        return False
    
    if label in {"PER", "LOC", "ORG"}:
        palabras = texto_limpio.split()

        if len(palabras) == 1 and texto_limpio.isupper():
            return False
    return True

def normalizar_entidad(texto: str) -> str:
    # Quitar saltos, tabs y espacios raros
    texto = texto.replace("\n", " ").replace("\r", " ").replace("\t", " ")
    texto = texto.replace("\u00a0", " ")

    # Quitar comillas decorativas
    texto = re.sub(r"[\"“”‘’]", "", texto)

    # Normalizar espacios múltiples
    texto = re.sub(r"\s+", " ", texto)

    # Quitar signos sobrantes en los extremos
    texto = texto.strip(" \t\n\r:;,.()[]{}")

    return texto.strip()

def eliminar_duplicados(entities):
        vistos = set()
        entidades_unicas = []

        for entity in entities:
            clave = (entity["text"].strip().lower(), entity["label"])

            if clave not in vistos:
                vistos.add(clave)
                entidades_unicas.append(entity)

        return entidades_unicas
def preparar_texto_para_spacy(texto: str) -> str:
    # Convierte saltos de línea y tabs en puntos para evitar que spaCy una campos distintos
    texto = re.sub(r"[\r\n\t]+", ". ", texto)

    # Normaliza espacios múltiples
    texto = re.sub(r"\s+", " ", texto)

    return texto.strip()

ROLE_ENTITY_PATTERNS = [
    ("DEMANDANTE", re.compile(r"\bDEMANDANTE\s*[:\-]\s*([A-ZÁÉÍÓÚÑ][A-Za-zÁÉÍÓÚÑáéíóúñ\s\.]{3,120}?)(?=\s*(?:,|\n|$|C\.?\s*I\.?|CI\b|con\b|domiciliad|mayor|de\s+\d|NIT\b))", re.IGNORECASE)),
    ("DEMANDADO", re.compile(r"\bDEMANDAD[OA]\s*[:\-]\s*([A-ZÁÉÍÓÚÑ][A-Za-zÁÉÍÓÚÑáéíóúñ\s\.]{3,120}?)(?=\s*(?:,|\n|$|C\.?\s*I\.?|CI\b|con\b|domiciliad|representad|mayor|de\s+\d|NIT\b))", re.IGNORECASE)),
    ("JUEZ", re.compile(r"\b(?:JUEZ|JUEZA)\s*[:\-]?\s*([A-ZÁÉÍÓÚÑ][A-Za-zÁÉÍÓÚÑáéíóúñ\s\.]{3,120}?)(?=\s*(?:,|\n|$|Juez|Jueza|del|de\s+la|de\s+lo))", re.IGNORECASE)),
    ("FISCAL", re.compile(r"\bFISCAL\s*[:\-]?\s*([A-ZÁÉÍÓÚÑ][A-Za-zÁÉÍÓÚÑáéíóúñ\s\.]{3,120}?)(?=\s*(?:,|\n|$|Ministerio|Fiscalía|Fiscalia|de\s+Materia))", re.IGNORECASE)),
    ("ABOGADO", re.compile(r"\b(?:ABOGAD[OA]|Abg\.?|ABG\.?)\s*[:\-]?\s*([A-ZÁÉÍÓÚÑ][A-Za-zÁÉÍÓÚÑáéíóúñ\s\.]{3,120}?)(?=\s*(?:,|\n|$|Matr[ií]cula|C\.A\.|NIT\b|C\.I\.|CI\b))", re.IGNORECASE)),
]

NRO_PROCESO_RE = re.compile(
    r"\b(?:N(?:[°ºo.]|ro\.?|umero|úmero)?\s*(?:de\s*)?(?:proceso|causa|expediente)|expediente|nurej|caso\s*n(?:[°ºo.]|ro\.?)?)\s*[:#\-]?\s*([A-Z0-9][A-Z0-9\-\/\.]{2,40})",
    re.IGNORECASE,
)

FECHA_AUDIENCIA_RE = re.compile(
    r"\b(?:fecha\s+de\s+audiencia|audiencia\s+(?:señalada|fijada|programada)?\s*(?:para\s+el|el|:)?|señala(?:se)?\s+audiencia\s+para\s+el)\s*([0-9]{1,2}[\/\-][0-9]{1,2}[\/\-][0-9]{2,4}|[0-9]{1,2}\s+de\s+[A-Za-záéíóúñÁÉÍÓÚÑ]+\s+de\s+[0-9]{4})",
    re.IGNORECASE,
)

JUZGADO_RE = re.compile(
    r"\b(Juzgado\s+[A-Za-zÁÉÍÓÚÑáéíóúñ°ºNn0-9\.\-\s]{3,120}?)(?=\s*(?:,|\n|$|de\s+la\s+ciudad|sito|dentro|en\s+fecha))",
    re.IGNORECASE,
)


def extraer_entidades_rol(text: str) -> list[dict]:
    entidades = []
    for label, rx in ROLE_ENTITY_PATTERNS:
        for m in rx.finditer(text):
            valor = normalizar_entidad(m.group(1))
            if len(valor) < 3:
                continue
            # evitar capturar etiquetas de sección
            if valor.upper() in {"CIVIL", "PENAL", "PUBLICO", "PÚBLICO", "INSTRUCCION", "INSTRUCCIÓN"}:
                continue
            entidades.append({"text": valor, "label": label})

    for m in NRO_PROCESO_RE.finditer(text):
        valor = normalizar_entidad(m.group(1) if m.lastindex else m.group(0))
        if valor and len(valor) >= 3:
            entidades.append({"text": valor, "label": "NRO_PROCESO"})

    for m in FECHA_AUDIENCIA_RE.finditer(text):
        valor = normalizar_entidad(m.group(1))
        if valor:
            entidades.append({"text": valor, "label": "FECHA_AUDIENCIA"})

    for m in JUZGADO_RE.finditer(text):
        valor = normalizar_entidad(m.group(1))
        if valor and es_entidad_valida(valor, "INSTITUCION"):
            entidades.append({"text": valor, "label": "JUZGADO"})

    return entidades


def find_entities(text, eliminar_repetidos=True, incluir_genericas=False):
    entities = []
    rangos_ocupados = []

    # 1. Detectar listas de artículos con regex sobre el texto original
    for match in ARTICLE_LIST_RE.finditer(text):
        articulo_limpio = limpiar_articulo(match.group())

        if es_entidad_valida(articulo_limpio, "ARTICULO"):
            entities.append({
                "text": articulo_limpio,
                "label": "ARTICULO"
            })
            rangos_ocupados.append(match.span())

    # 2. Preparar texto para spaCy evitando que una campos separados por saltos de línea
    texto_para_spacy = preparar_texto_para_spacy(text)

    # 3. Procesar con spaCy
    doc = nlp(texto_para_spacy)
    
    etiquetas_permitidas = set(ETIQUETAS_JURIDICAS)

    if incluir_genericas:
        etiquetas_permitidas = etiquetas_permitidas | ETIQUETAS_GENERICAS

    for ent in doc.ents:
        # Solo aceptar etiquetas que realmente quieres devolver
        if ent.label_ not in etiquetas_permitidas:
            continue

        # Los artículos ya los estamos manejando mejor con regex
        if ent.label_ == "ARTICULO":
            continue

        texto_normalizado = normalizar_entidad(ent.text)

        if es_entidad_valida(texto_normalizado, ent.label_):
            entities.append({
                "text": texto_normalizado,
                "label": ent.label_
            })

    # 4. Entidades jurídicas por rol/patrón (demanda, proceso, audiencia, juzgado)
    for e in extraer_entidades_rol(text):
        if e["label"] in etiquetas_permitidas and es_entidad_valida(e["text"], e["label"]):
            entities.append(e)

    if eliminar_repetidos:
        return eliminar_duplicados_semanticos(entities)

    return entities

def limpiar_articulo(texto_articulo):
    # Quita aclaraciones entre paréntesis, por ejemplo: "(Difamación)"
    texto_articulo = re.sub(r"\s*\([^)]*\)", "", texto_articulo)

    # Normaliza espacios múltiples
    texto_articulo = re.sub(r"\s+", " ", texto_articulo)

    return texto_articulo.strip()

# Ejemplo de prueba
# if __name__ == "__main__":