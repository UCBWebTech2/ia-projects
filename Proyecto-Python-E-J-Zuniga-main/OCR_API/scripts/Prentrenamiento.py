import pandas as pd
from sklearn.model_selection import train_test_split
from pathlib import Path
from OCR_API.app.core.config import settings

# Ruta del archivo CSV
dataset_path = settings.dataset_path

# Cargar el conjunto de datos
df = pd.read_csv(dataset_path)

# Verifica si las columnas están correctamente cargadas
if 'texto' not in df.columns or 'etiqueta' not in df.columns:
    raise ValueError("El archivo CSV debe contener las columnas 'texto' y 'etiqueta'.")

# Dividir en características (X) y etiquetas (y)
X = df['texto']  # Textos
y = df['etiqueta']  # Etiquetas (penal, civil)

# Dividir en datos de entrenamiento y prueba (80% - 20%)
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42, stratify=y)

# Guardar los conjuntos en archivos CSV
train_path = Path(dataset_path).parent / "train_dataset.csv"
test_path = Path(dataset_path).parent / "test_dataset.csv"

pd.DataFrame({'texto': X_train, 'etiqueta': y_train}).to_csv(train_path, index=False)
pd.DataFrame({'texto': X_test, 'etiqueta': y_test}).to_csv(test_path, index=False)

print(f"Datos de entrenamiento guardados en: {train_path}")
print(f"Datos de prueba guardados en: {test_path}")
