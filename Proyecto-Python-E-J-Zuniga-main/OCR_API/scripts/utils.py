from pathlib import Path
from constants import FILES_PATH

def split_list(separator, objects):
    """Divide una lista de objetos utilizando el separador dado."""
    new_list = []
    for i in objects:
        separated = i.split(separator)
        new_list.extend([element.rstrip() for element in separated])
    return new_list

def get_file_path(filename):
    """Genera la ruta completa del archivo dado su nombre."""
    files_folder = Path(FILES_PATH)
    return files_folder / filename

def clean_list(list_to_update, list_to_be_removed):
    """Elimina elementos de list_to_update que están en list_to_be_removed."""
    return [x for x in list_to_update if x not in list_to_be_removed]

def get_legal_terms(objects):
    """Filtra y devuelve términos legales específicos si es necesario."""
    # Por ejemplo, filtrar términos que contienen "QUERELLA" o "ACUSACIÓN"
    an_iterator = filter(lambda word: any(term in word for term in ["QUERELLA", "ACUSACIÓN"]), objects)
    return list(an_iterator)
