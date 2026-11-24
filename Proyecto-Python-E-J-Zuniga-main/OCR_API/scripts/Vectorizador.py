import pandas as pd
from sklearn.feature_extraction.text import CountVectorizer
import pickle
from pathlib import Path
from OCR_API.app.core.config import settings

# Cargar los datos de entrenamiento
train_path = settings.files_dir / "train_dataset.csv"
train_df = pd.read_csv(train_path)

# Verificar columnas
if 'texto' not in train_df.columns:
    raise ValueError("El archivo de entrenamiento debe contener la columna 'texto'.")

# Vectorización del texto usando CountVectorizer
vectorizer = CountVectorizer()
X_train_vectorized = vectorizer.fit_transform(train_df['texto'])

# Guardar el vectorizador
vectorizer_path = settings.vectorizer_path
with open(vectorizer_path, "wb") as vectorizer_file:
    pickle.dump(vectorizer, vectorizer_file)

print(f"Vectorizador guardado exitosamente en: {vectorizer_path}")
