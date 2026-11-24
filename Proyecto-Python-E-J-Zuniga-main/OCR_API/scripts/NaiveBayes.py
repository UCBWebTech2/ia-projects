import spacy
from sklearn.feature_extraction.text import CountVectorizer
from sklearn.naive_bayes import MultinomialNB
from sklearn.model_selection import train_test_split
from sklearn.metrics import classification_report
import pickle
import logging
from pathlib import Path

logging.basicConfig(level=logging.DEBUG)

MODELS_DIR = Path(__file__).resolve().parents[1] / "models"

class NaiveBayesMN:
    def __init__(self):
        self.nlp = spacy.load("es_core_news_sm")
        self.vectorizer = CountVectorizer(stop_words='spanish', max_features=5000)
        #Explicar
        self.model = MultinomialNB()

    def preprocess_text(self, text: str) -> str:
        doc = self.nlp(text.lower())
        tokens = [token.lemma_ for token in doc if not token.is_stop and not token.is_punct]
        return " ".join(tokens)

    def train_model(self, data):
        
        logging.info("Iniciando preprocesamiento de datos...")
        texts, categories = zip(*data)
        processed_texts = [self.preprocess_text(text) for text in texts]

        logging.info("Convirtiendo textos a vectores...")
        X = self.vectorizer.fit_transform(processed_texts)

        # Dividir en conjunto de entrenamiento y prueba
        X_train, X_test, y_train, y_test = train_test_split(X, categories, test_size=0.2, random_state=42)

        logging.info("Entrenando el modelo...")
        self.model.fit(X_train, y_train)

        logging.info("Evaluando el modelo...")
        y_pred = self.model.predict(X_test)
        print(classification_report(y_test, y_pred))

    def classify_text(self, text: str) -> dict:
        
        processed_text = self.preprocess_text(text)
        X = self.vectorizer.transform([processed_text])

        predicted_class = self.model.predict(X)[0]
        confidence = max(self.model.predict_proba(X)[0])

        return {"document_type": predicted_class, "confidence": confidence}

    def save_model(
        self,
        model_path=MODELS_DIR / "naive_bayes_model.pkl",
        vectorizer_path=MODELS_DIR / "vectorizer.pkl",
    ):
        """
        Guardar modelo y vectorizador en archivos pickle.
        """
        Path(model_path).parent.mkdir(parents=True, exist_ok=True)
        Path(vectorizer_path).parent.mkdir(parents=True, exist_ok=True)
        with open(model_path, "wb") as model_file, open(vectorizer_path, "wb") as vectorizer_file:
            pickle.dump(self.model, model_file)
            pickle.dump(self.vectorizer, vectorizer_file)
        logging.info(f"Modelo guardado en {model_path} y vectorizador en {vectorizer_path}")

    def load_model(
        self,
        model_path=MODELS_DIR / "naive_bayes_model.pkl",
        vectorizer_path=MODELS_DIR / "vectorizer.pkl",
    ):

        with open(model_path, "rb") as model_file, open(vectorizer_path, "rb") as vectorizer_file:
            self.model = pickle.load(model_file)
            self.vectorizer = pickle.load(vectorizer_file)
        logging.info("Modelo y vectorizador cargados correctamente.")
