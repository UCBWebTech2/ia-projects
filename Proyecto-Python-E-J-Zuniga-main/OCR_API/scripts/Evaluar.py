import pandas as pd
import pickle
from sklearn.metrics import classification_report, accuracy_score
from OCR_API.app.core.config import settings

# Cargar el conjunto de prueba
test_path = settings.files_dir / "test_dataset.csv"
test_df = pd.read_csv(test_path)

# Cargar el vectorizador
vectorizer_path = settings.vectorizer_path
with open(vectorizer_path, "rb") as vectorizer_file:
    vectorizer = pickle.load(vectorizer_file)

# Cargar el modelo entrenado
model_path = settings.model_path
with open(model_path, "rb") as model_file:
    model = pickle.load(model_file)

# Vectorizar los datos de prueba
X_test_vectorized = vectorizer.transform(test_df['texto'])
y_test = test_df['etiqueta']

# Realizar predicciones
y_pred = model.predict(X_test_vectorized)

# Evaluar desempeño
print("Reporte de clasificación:")
print(classification_report(y_test, y_pred))
print(f"Precisión del modelo: {accuracy_score(y_test, y_pred):.2f}")
