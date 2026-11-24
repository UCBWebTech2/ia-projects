import pandas as pd
import re
import pickle
from sklearn.feature_extraction.text import CountVectorizer
from OCR_API.app.core.config import settings

# Ruta del dataset de entrenamiento
CSV_PATH = settings.files_dir / "train_4000.csv"

# Función de limpieza básica
def limpiar_texto(texto):
    texto = str(texto).lower()
    texto = re.sub(r"[^\w\s]", "", texto)  # quitar puntuación
    texto = re.sub(r"\d+", "", texto)      # quitar números
    return texto

def main():
    print("📥 Cargando dataset...")
    df = pd.read_csv(CSV_PATH)

    if "texto" not in df.columns:
        raise ValueError("El archivo debe tener una columna llamada 'texto'.")

    print("🧽 Preprocesando textos...")
    textos_limpios = df["texto"].apply(limpiar_texto)

    print("🧠 Entrenando CountVectorizer...")
    # vectorizer = CountVectorizer(stop_words='spanish', max_features=5000)
    spanish_stopwords = [
    "de", "la", "que", "el", "en", "y", "a", "los", "del", "se", "las", "por",
    "un", "para", "con", "no", "una", "su", "al", "lo", "como", "más", "pero",
    "sus", "le", "ya", "o", "este", "sí", "porque", "esta", "entre", "cuando"
]
    vectorizer = CountVectorizer(stop_words=spanish_stopwords, max_features=5000)

    vectorizer.fit(textos_limpios)

    print(f"✅ Vocabulario generado con {len(vectorizer.vocabulary_)} términos.")

    print("💾 Guardando vectorizador...")
    with open(settings.vectorizer_path, "wb") as f:
        pickle.dump(vectorizer, f)

    print("🎉 Vectorizador guardado como 'vectorizer.pkl'")

if __name__ == "__main__":
    main()
