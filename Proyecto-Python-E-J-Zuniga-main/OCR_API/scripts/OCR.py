import logging
import re
import unicodedata
from difflib import SequenceMatcher
from typing import Any

import cv2
import numpy as np
import pytesseract

logger = logging.getLogger("uvicorn.error")


OCR_CONFIGS_PRINCIPALES = {
    "gray_default_actual": "",
    "gray_psm3": "--oem 3 --psm 3",
    "gray_psm4": "--oem 3 --psm 4",
    "gray_psm6": "--oem 3 --psm 6",
}

OCR_CONFIGS_SECUNDARIOS = {
    "gray_psm11": "--oem 3 --psm 11",
}

OCR_CONFIGS_AUTO = {
    **OCR_CONFIGS_PRINCIPALES,
    **OCR_CONFIGS_SECUNDARIOS,
}


OCR_DESCRIPCIONES_AUTO = {
    "gray_default_actual": "Escala de grises, sin config. Método actual.",
    "gray_psm3": "Escala de grises con --oem 3 --psm 3.",
    "gray_psm4": "Escala de grises con --oem 3 --psm 4.",
    "gray_psm6": "Escala de grises con --oem 3 --psm 6.",
    "gray_psm11": "Escala de grises con --oem 3 --psm 11. Se usa solo como apoyo.",
}


PALABRAS_JURIDICAS_OCR = [
    "tribunal",
    "juzgado",
    "juez",
    "fiscal",
    "ministerio público",
    "ministerio publico",
    "código",
    "codigo",
    "ley",
    "art",
    "artículo",
    "articulo",
    "procedimiento",
    "penal",
    "civil",
    "sentencia",
    "resolución",
    "resolucion",
    "proceso",
    "demanda",
    "mandamiento",
    "audiencia",
    "constitución",
    "constitucion",
    "estado plurinacional",
    "bolivia",
    "órgano judicial",
    "organo judicial",
]


def normalizar_linea_ocr(linea: str) -> str:
    texto = linea.lower().strip()

    texto = unicodedata.normalize("NFD", texto)
    texto = "".join(
        caracter
        for caracter in texto
        if unicodedata.category(caracter) != "Mn"
    )

    texto = re.sub(r"\s+", " ", texto)
    texto = re.sub(r"[^a-z0-9 ]", " ", texto)
    texto = re.sub(r"\s+", " ", texto)

    return texto.strip()


def contiene_palabra_juridica(texto: str) -> bool:
    texto_normalizado = normalizar_linea_ocr(texto)

    return any(
        normalizar_linea_ocr(palabra) in texto_normalizado
        for palabra in PALABRAS_JURIDICAS_OCR
    )


def puntuar_linea_ocr(linea: str) -> float:
    if not linea or not linea.strip():
        return -100

    linea_limpia = linea.strip()
    linea_normalizada = normalizar_linea_ocr(linea_limpia)

    if not linea_normalizada:
        return -100

    score = 0.0

    palabras = re.findall(r"[a-z0-9]{3,}", linea_normalizada)
    score += len(palabras) * 3

    if contiene_palabra_juridica(linea_limpia):
        score += 15

    caracteres_validos = sum(
        1
        for caracter in linea_limpia
        if caracter.isalnum()
        or caracter.isspace()
        or caracter in ".,;:()/-°ºáéíóúÁÉÍÓÚñÑüÜ*"
    )

    proporcion_valida = caracteres_validos / max(len(linea_limpia), 1)

    if proporcion_valida < 0.70:
        score -= 25

    simbolos_raros = len(
        re.findall(r"[|_=<>~^{}\[\]\\]", linea_limpia)
    )
    score -= simbolos_raros * 5

    if len(linea_limpia) < 4:
        score -= 20

    if len(palabras) == 0:
        score -= 30

    if re.fullmatch(r"[\W_0-9\s]+", linea_limpia):
        score -= 50

    return score

def clave_visual_linea(item: dict[str, Any]) -> tuple[int, int]:
    """
    Agrupa líneas por bandas verticales para evitar que pequeñas diferencias
    de top desordenen el texto.
    """
    banda_vertical = int(item.get("top", 0) // 25)
    return banda_vertical, int(item.get("left", 0))

def ejecutar_ocr_string(imagen_gris: np.ndarray, config: str = "") -> str:
    if config:
        return pytesseract.image_to_string(
            imagen_gris,
            lang="spa",
            config=config
        ).strip()

    return pytesseract.image_to_string(
        imagen_gris,
        lang="spa"
    ).strip()


def evaluar_texto_ocr(texto: str) -> float:
    if not texto or not texto.strip():
        return -1000

    texto_limpio = texto.strip()
    texto_lower = texto_limpio.lower()

    cantidad_caracteres = len(texto_limpio)

    palabras = re.findall(
        r"[A-Za-zÁÉÍÓÚÜÑáéíóúüñ]{3,}",
        texto_limpio
    )

    cantidad_palabras = len(palabras)

    palabras_juridicas = sum(
        1
        for palabra in PALABRAS_JURIDICAS_OCR
        if palabra in texto_lower
    )

    caracteres_validos = sum(
        1
        for caracter in texto_limpio
        if caracter.isalnum()
        or caracter.isspace()
        or caracter in ".,;:()/-°ºáéíóúÁÉÍÓÚñÑüÜ*"
    )

    proporcion_valida = caracteres_validos / max(cantidad_caracteres, 1)

    simbolos_raros = len(
        re.findall(r"[|_=<>~^{}\[\]\\]{2,}", texto_limpio)
    )

    lineas_muy_cortas = sum(
        1
        for linea in texto_limpio.splitlines()
        if 0 < len(linea.strip()) <= 2
    )

    score = 0.0

    score += cantidad_palabras * 1.2
    score += palabras_juridicas * 20
    score += proporcion_valida * 80
    score -= simbolos_raros * 15
    score -= lineas_muy_cortas * 2

    if cantidad_caracteres < 80:
        score -= 50

    return score


def son_lineas_similares(linea_a: str, linea_b: str) -> bool:
    normalizada_a = normalizar_linea_ocr(linea_a)
    normalizada_b = normalizar_linea_ocr(linea_b)

    if not normalizada_a or not normalizada_b:
        return False

    if normalizada_a == normalizada_b:
        return True

    similitud_caracteres = SequenceMatcher(
        None,
        normalizada_a,
        normalizada_b
    ).ratio()

    similitud_palabras = porcentaje_palabras_comunes(
        normalizada_a,
        normalizada_b
    )

    diferencia_longitud = abs(
        len(normalizada_a) - len(normalizada_b)
    )

    longitud_mayor = max(
        len(normalizada_a),
        len(normalizada_b),
        1
    )

    proporcion_diferencia = diferencia_longitud / longitud_mayor

    return (
        similitud_caracteres >= 0.86
        and similitud_palabras >= 0.65
        and proporcion_diferencia <= 0.45
    )

def buscar_indice_linea_equivalente(
    item: dict[str, Any],
    lineas_seleccionadas: list[dict[str, Any]]
) -> int:
    for indice, existente in enumerate(lineas_seleccionadas):
        misma_linea_por_texto = son_lineas_similares(
            item["linea"],
            existente["linea"]
        )

        distancia_vertical = abs(
            int(item.get("top", 0)) - int(existente.get("top", 0))
        )

        cerca_verticalmente = distancia_vertical <= 35

        if misma_linea_por_texto and cerca_verticalmente:
            return indice

    return -1
def agregar_o_reemplazar_linea_con_orden(
    item: dict[str, Any],
    lineas_seleccionadas: list[dict[str, Any]],
    orden_por_defecto: float,
    permitir_agregar_nueva: bool = True
) -> None:
    indice = buscar_indice_linea_equivalente(
        item,
        lineas_seleccionadas
    )

    if indice == -1:
        if permitir_agregar_nueva:
            nueva = dict(item)
            nueva["orden_lectura"] = orden_por_defecto
            lineas_seleccionadas.append(nueva)

        return

    linea_actual = lineas_seleccionadas[indice]

    if item["score"] > linea_actual["score"] + 3:
        nueva = dict(item)

        # Esto es lo más importante:
        # conserva el orden de la línea original.
        nueva["orden_lectura"] = linea_actual["orden_lectura"]

        lineas_seleccionadas[indice] = nueva

def porcentaje_palabras_comunes(texto_a: str, texto_b: str) -> float:
    normalizada_a = normalizar_linea_ocr(texto_a)
    normalizada_b = normalizar_linea_ocr(texto_b)

    palabras_a = set(normalizada_a.split())
    palabras_b = set(normalizada_b.split())

    if not palabras_a or not palabras_b:
        return 0.0

    comunes = palabras_a & palabras_b

    return len(comunes) / max(min(len(palabras_a), len(palabras_b)), 1)

def parsear_confianza_ocr(valor_confianza: Any) -> float:
    try:
        confianza = float(valor_confianza)

        if confianza < 0:
            return 0.0

        return confianza

    except Exception:
        return 0.0


def extraer_lineas_con_confianza(
    imagen_gris: np.ndarray,
    metodo: str,
    config: str
) -> list[dict[str, Any]]:
    data = pytesseract.image_to_data(
        imagen_gris,
        lang="spa",
        config=config,
        output_type=pytesseract.Output.DICT
    )

    grupos: dict[tuple[Any, Any, Any], dict[str, Any]] = {}

    textos = data.get("text", [])
    total_items = len(textos)

    for i in range(total_items):
        palabra = str(data["text"][i]).strip()

        if not palabra or palabra.lower() == "nan":
            continue

        confianza = parsear_confianza_ocr(data["conf"][i])

        block_num = data["block_num"][i]
        par_num = data["par_num"][i]
        line_num = data["line_num"][i]

        clave = (block_num, par_num, line_num)

        if clave not in grupos:
            grupos[clave] = {
                "palabras": [],
                "confianzas": [],
                "lefts": [],
                "tops": []
            }

        grupos[clave]["palabras"].append((data["left"][i], palabra))
        grupos[clave]["confianzas"].append(confianza)
        grupos[clave]["lefts"].append(data["left"][i])
        grupos[clave]["tops"].append(data["top"][i])

    lineas: list[dict[str, Any]] = []

    for grupo in grupos.values():
        palabras_ordenadas = sorted(
            grupo["palabras"],
            key=lambda item: item[0]
        )

        texto_linea = " ".join(
            palabra
            for _, palabra in palabras_ordenadas
        ).strip()

        if not texto_linea:
            continue

        confianza_promedio = sum(grupo["confianzas"]) / max(
            len(grupo["confianzas"]),
            1
        )

        score_base = puntuar_linea_ocr(texto_linea)
        score_total = score_base + (confianza_promedio * 0.5)

        lineas.append({
            "linea": texto_linea,
            "metodo": metodo,
            "confianza": round(confianza_promedio, 2),
            "score": round(score_total, 2),
            "top": min(grupo["tops"]) if grupo["tops"] else 0,
            "left": min(grupo["lefts"]) if grupo["lefts"] else 0,
            "contieneJuridica": contiene_palabra_juridica(texto_linea)
        })

    return lineas


def buscar_indice_linea_similar(
    linea: str,
    lineas_seleccionadas: list[dict[str, Any]]
) -> int:
    for indice, item in enumerate(lineas_seleccionadas):
        if son_lineas_similares(linea, item["linea"]):
            return indice

    return -1


def agregar_o_reemplazar_linea(
    item: dict[str, Any],
    lineas_seleccionadas: list[dict[str, Any]],
    permitir_reemplazo: bool = True
) -> None:
    indice_similar = buscar_indice_linea_similar(
        item["linea"],
        lineas_seleccionadas
    )

    if indice_similar == -1:
        lineas_seleccionadas.append(item)
        return

    if not permitir_reemplazo:
        return

    linea_actual = lineas_seleccionadas[indice_similar]

    if item["score"] > linea_actual["score"] + 3:
        lineas_seleccionadas[indice_similar] = item


def combinar_lineas_con_confianza_desde_imagen(
    imagen_gris: np.ndarray,
    metodo_principal: str,
    modo_conservador: bool = False
) -> tuple[str, list[dict[str, Any]]]:
    todas_las_lineas: list[dict[str, Any]] = []

    for metodo, config in OCR_CONFIGS_AUTO.items():
        try:
            lineas_metodo = extraer_lineas_con_confianza(
                imagen_gris,
                metodo,
                config
            )

            todas_las_lineas.extend(lineas_metodo)

        except Exception:
            logger.exception(
                f"Error extrayendo líneas OCR con método {metodo}."
            )

    lineas_seleccionadas: list[dict[str, Any]] = []

    # 1. Usar el método principal como base del orden
    lineas_principales = [
        linea
        for linea in todas_las_lineas
        if linea["metodo"] == metodo_principal
    ]

    lineas_principales = sorted(
        lineas_principales,
        key=clave_visual_linea
    )

    for indice, item in enumerate(lineas_principales):
        texto_norm = normalizar_linea_ocr(item["linea"])
        cantidad_palabras = len(texto_norm.split())

        es_basura_principal = (
            not item["contieneJuridica"]
            and item["confianza"] < 50
            and item["score"] < 35
        )

        es_fragmento_muy_corto = (
            cantidad_palabras <= 1
            and len(texto_norm) < 10
        )

        if es_basura_principal or es_fragmento_muy_corto:
            continue

        nueva = dict(item)

        # El método principal define el orden base
        nueva["orden_lectura"] = indice * 10

        lineas_seleccionadas.append(nueva)

    # 2. Usar líneas secundarias solo para mejorar/completar
    lineas_secundarias = [
        linea
        for linea in todas_las_lineas
        if linea["metodo"] != metodo_principal
    ]

    # Se ordenan por score porque primero queremos usar las mejores
    lineas_secundarias = sorted(
        lineas_secundarias,
        key=lambda item: item["score"],
        reverse=True
    )

    for item in lineas_secundarias:
        es_linea_util = (
            item["score"] >= 45
            or (item["confianza"] >= 65 and item["score"] >= 30)
            or (item["contieneJuridica"] and item["score"] >= 28)
        )

        if not es_linea_util:
            continue

        indice_equivalente = buscar_indice_linea_equivalente(
            item,
            lineas_seleccionadas
        )

        if indice_equivalente != -1:
            linea_actual = lineas_seleccionadas[indice_equivalente]

            if item["score"] > linea_actual["score"] + 3:
                nueva = dict(item)
                # Mantener el orden de la línea que reemplaza
                nueva["orden_lectura"] = linea_actual["orden_lectura"]
                lineas_seleccionadas[indice_equivalente] = nueva
            continue
            
        if modo_conservador:
            continue
        texto_norm = normalizar_linea_ocr(item["linea"])
        cantidad_palabras = len(texto_norm.split())

        es_fragmento_corto = cantidad_palabras <= 2 and len(texto_norm) < 18
        if item["metodo"] == "gray_psm11":
            continue
        es_linea_nueva_muy_confiable = (
            not es_fragmento_corto
            and item["metodo"] != "gray_psm11"
            and (
                item["score"] >= 75
                or (item["confianza"] >= 85 and item["score"] >= 60)
                or (item["contieneJuridica"] and item["score"] >= 60 and cantidad_palabras >= 4)
            )
        )

        if not es_linea_nueva_muy_confiable:
            continue

        # Insertar según posición visual, pero sin alterar el orden base
        posicion_visual = clave_visual_linea(item)

        posicion_insertar = len(lineas_seleccionadas)

        for indice, linea_existente in enumerate(lineas_seleccionadas):
            if clave_visual_linea(linea_existente) > posicion_visual:
                posicion_insertar = indice
                break

        nueva = dict(item)

        if posicion_insertar == 0:
            nueva["orden_lectura"] = (
                lineas_seleccionadas[0]["orden_lectura"] - 1
                if lineas_seleccionadas
                else 0
            )

        elif posicion_insertar >= len(lineas_seleccionadas):
            nueva["orden_lectura"] = (
                lineas_seleccionadas[-1]["orden_lectura"] + 1
                if lineas_seleccionadas
                else 0
            )

        else:
            orden_anterior = lineas_seleccionadas[posicion_insertar - 1]["orden_lectura"]
            orden_siguiente = lineas_seleccionadas[posicion_insertar]["orden_lectura"]

            nueva["orden_lectura"] = (
                orden_anterior + orden_siguiente
            ) / 2

        lineas_seleccionadas.append(nueva)

    # 4. Orden final por orden_lectura, NO por top/left directo
    lineas_seleccionadas = sorted(
        lineas_seleccionadas,
        key=lambda item: item["orden_lectura"]
    )

    texto_final = "\n".join(
        item["linea"]
        for item in lineas_seleccionadas
    )

    return texto_final.strip(), lineas_seleccionadas



def _extract_text_from_image_auto_base(image_bytes: bytes) -> dict[str, Any]:
    image = cv2.imdecode(
        np.frombuffer(image_bytes, np.uint8),
        cv2.IMREAD_COLOR
    )

    if image is None:
        raise ValueError("No se pudo decodificar la imagen para OCR automático.")

    gray_image = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)

    resultados = {}

    # 1. Ejecutar varios OCR completos
    for metodo, config in OCR_CONFIGS_AUTO.items():
        texto = ejecutar_ocr_string(gray_image, config)
        score = evaluar_texto_ocr(texto)

        resultados[metodo] = {
            "descripcion": OCR_DESCRIPCIONES_AUTO.get(metodo, metodo),
            "cantidad_caracteres": len(texto),
            "score": round(score, 2),
            "texto": texto
        }

    # 2. Elegir método principal
    metodo_principal = max(
        resultados,
        key=lambda metodo: resultados[metodo]["score"]
    )

    score_principal = resultados[metodo_principal]["score"]

    score_psm6 = resultados.get("gray_psm6", {}).get("score", 0)

    if (
        metodo_principal != "gray_psm6"
        and score_psm6 >= score_principal * 0.97
    ):
        metodo_principal = "gray_psm6"
        score_principal = score_psm6
        
    modo_conservador = score_principal >= 500

    # 3. Combinar líneas buenas usando image_to_data
    texto_combinado, lineas_usadas = combinar_lineas_con_confianza_desde_imagen(
        gray_image,
        metodo_principal,
        modo_conservador=modo_conservador
    )

    # 4. Si la combinación sale vacía, usar el método principal
    if not texto_combinado:
        texto_combinado = resultados[metodo_principal]["texto"]

    score_combinado = evaluar_texto_ocr(texto_combinado)

    return {
        "texto": texto_combinado,
        "cantidadCaracteres": len(texto_combinado),
        "metodoPrincipal": metodo_principal,
        "estrategiaFinal": "selector_automatico_con_combinacion_por_lineas_con_confianza",
        "scorePrincipal": score_principal,
        "scoreTextoCombinado": round(score_combinado, 2),
        "cantidadLineasUsadas": len(lineas_usadas),
        "lineasUsadas": lineas_usadas,
        "resultadosBase": resultados
    }


def extract_text_from_image_auto(image_bytes: bytes) -> str:
    detalle = _extract_text_from_image_auto_base(image_bytes)
    return detalle["texto"]


def extract_text_from_image_auto_detalle(image_bytes: bytes) -> dict[str, Any]:
    return _extract_text_from_image_auto_base(image_bytes)