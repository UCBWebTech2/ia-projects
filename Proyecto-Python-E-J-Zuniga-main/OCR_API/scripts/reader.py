from constants import (
    FILES_PATH,
    MINED_OBJECTS_FILE_NAME,
    TRAINING_EXAMPLES_FILE_NAME,
    FILE_SEPARATOR
)
from utils import split_list, clean_list

def get_mined_objects():
    legal_terms = []
    with open(get_file_path(MINED_OBJECTS_FILE_NAME), 'r', encoding="utf8") as file:
        data = file.read().splitlines()
        # Suponiendo que los términos legales están en el archivo separados por FILE_SEPARATOR
        legal_terms = split_list(FILE_SEPARATOR, data)
    
    return clean_list(legal_terms)

def get_examples():
    lines = []
    with open(get_file_path(TRAINING_EXAMPLES_FILE_NAME), 'r', encoding="utf8") as file:
        for line in file:
            if len(line.split()) == 0:
                continue
            lines.append(line.rstrip())
    return lines

def get_file_path(file_name):
    """Genera la ruta completa del archivo dado su nombre."""
    return f"{FILES_PATH}{file_name}"
