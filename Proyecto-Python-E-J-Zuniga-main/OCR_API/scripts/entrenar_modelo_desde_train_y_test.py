import pandas as pd
import pickle
from sklearn.naive_bayes import MultinomialNB
from sklearn.metrics import classification_report
from OCR_API.app.core.config import settings

TRAIN_PATH = settings.files_dir / "train_4000.csv"
TEST_PATH = settings.files_dir / "test_4000.csv"
VECTORIZER_PATH = settings.vectorizer_path
MODEL_PATH = settings.model_path

def main():
    print("Cargando datos de entrenamiento y prueba...")
    train_df = pd.read_csv(TRAIN_PATH)
    test_df = pd.read_csv(TEST_PATH)

    print("Cargando vectorizador...")
    with open(VECTORIZER_PATH, "rb") as f:
        vectorizer = pickle.load(f)

    print("Vectorizando textos...")
    X_train = vectorizer.transform(train_df["texto"])
    y_train = train_df["etiqueta"]

    X_test = vectorizer.transform(test_df["texto"])
    y_test = test_df["etiqueta"]

    print("Entrenando modelo Naive Bayes...")
    model = MultinomialNB()
    model.fit(X_train, y_train)

    print("Evaluando modelo...")
    y_pred = model.predict(X_test)
    print(classification_report(y_test, y_pred))

    print("Guardando modelo entrenado...")
    with open(MODEL_PATH, "wb") as f:
        pickle.dump(model, f)

    print(f"✅ Modelo guardado en: {MODEL_PATH}")

if __name__ == "__main__":
    main()
