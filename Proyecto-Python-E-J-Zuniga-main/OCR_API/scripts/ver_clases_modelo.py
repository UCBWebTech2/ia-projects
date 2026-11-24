import pickle

from OCR_API.app.core.config import settings


with settings.model_path.open("rb") as model_file:
    model = pickle.load(model_file)

print("Clases aprendidas por el modelo:", model.classes_)
