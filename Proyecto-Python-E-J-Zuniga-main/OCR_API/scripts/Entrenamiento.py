import pandas as pd
import pickle
from sklearn.feature_extraction.text import CountVectorizer
from sklearn.naive_bayes import MultinomialNB
from OCR_API.app.core.config import settings

# Cargar los datos de entrenamiento
train_path = settings.files_dir / "train_dataset.csv"
train_df = pd.read_csv(train_path)

# Verificar columnas
if 'texto' not in train_df.columns or 'etiqueta' not in train_df.columns:
    raise ValueError("El archivo de entrenamiento debe contener las columnas 'texto' y 'etiqueta'.")

# Cargar el vectorizador
vectorizer_path = settings.vectorizer_path
with open(vectorizer_path, "rb") as vectorizer_file:
    vectorizer = pickle.load(vectorizer_file)

# Vectorizar los datos de entrenamiento
X_train_vectorized = vectorizer.transform(train_df['texto'])
y_train = train_df['etiqueta']

# Entrenar el modelo Naive Bayes
model = MultinomialNB()
model.fit(X_train_vectorized, y_train)

# Guardar el modelo entrenado
model_path = settings.model_path
with open(model_path, "wb") as model_file:
    pickle.dump(model, model_file)

print(f"Modelo Naive Bayes entrenado y guardado en: {model_path}")
