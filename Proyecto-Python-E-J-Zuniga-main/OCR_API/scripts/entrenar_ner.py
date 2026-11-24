import spacy
from spacy.training.example import Example
from OCR_API.files.training_ner_data import TRAIN_DATA as TRAINING_DATA

nlp = spacy.load("es_core_news_sm")

# Obtener el componente NER
ner = nlp.get_pipe("ner")

# Añadir nuevas etiquetas
for _, annotations in TRAINING_DATA:
    for ent in annotations.get("entities"):
        ner.add_label(ent[2])

# Desactivar otros componentes durante el entrenamiento
other_pipes = [pipe for pipe in nlp.pipe_names if pipe != "ner"]

# Entrenar el modelo
import random
from pathlib import Path

with nlp.disable_pipes(*other_pipes):  # solo entrena NER
    optimizer = nlp.resume_training()
    for iteration in range(30):  # ajuste del número de iteraciones
        random.shuffle(TRAINING_DATA)
        losses = {}

        for text, annotations in TRAINING_DATA:
            doc = nlp.make_doc(text)
            example = Example.from_dict(doc, annotations)
            nlp.update([example], drop=0.35, losses=losses)

        print(f"Iteración {iteration + 1}, pérdidas: {losses}")

# Guardar el modelo entrenado
output_dir = Path("C:/Users/Miguel Angel/Documents/PFU/PFU/PFPYEstudioJuridico/OCR_API/modelos/ner_personalizado")
nlp.to_disk(output_dir)
print(f"Modelo guardado en: {output_dir}")