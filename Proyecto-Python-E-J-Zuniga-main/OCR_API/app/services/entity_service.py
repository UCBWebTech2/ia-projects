from OCR_API.scripts.EntityRuler import (
    eliminar_duplicados_semanticos,
    find_entities,
)


def count_entities(text: str, include_generic: bool = False) -> int:
    if not text or not text.strip():
        return 0

    entities = find_entities(
        text.strip(),
        eliminar_repetidos=False,
        incluir_genericas=include_generic,
    )
    return len(
        eliminar_duplicados_semanticos(entities, incluir_frecuencia=True)
    )


def extract_grouped_entities(
    text: str,
    include_generic: bool = False,
) -> list[dict]:
    raw_entities = find_entities(
        text,
        eliminar_repetidos=False,
        incluir_genericas=include_generic,
    )
    grouped = eliminar_duplicados_semanticos(
        raw_entities,
        incluir_frecuencia=True,
    )
    return [
        {
            "Termino": entity["text"],
            "TipoTermino": entity["label"],
            "Frecuencia": entity["frequency"],
        }
        for entity in grouped
    ]


def extract_entities(text: str, include_generic: bool = False) -> list[dict]:
    return find_entities(text, incluir_genericas=include_generic)
