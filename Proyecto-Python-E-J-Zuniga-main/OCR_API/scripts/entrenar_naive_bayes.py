import pandas as pd
from NaiveBayes import NaiveBayesMN
from OCR_API.app.core.config import settings

# Ruta del dataset
DATASET_PATH = settings.files_dir / "dataset_derecho_4k.csv"

def main():
    print("📥 Cargando dataset...")
    df = pd.read_csv(DATASET_PATH)

    print("Formateando datos...")
    data = list(zip(df["texto"], df["etiqueta"]))

    print("Inicializando modelo Naive Bayes...")
    modelo = NaiveBayesMN()

    print("Entrenando modelo...")
    modelo.train_model(data)

    print("Guardando modelo entrenado y vectorizador...")
    modelo.save_model(settings.model_path, settings.vectorizer_path)

    print("¡Entrenamiento completo!")

if __name__ == "__main__":
    main()
