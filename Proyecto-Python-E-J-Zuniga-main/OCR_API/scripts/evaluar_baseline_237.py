import hashlib
import io
import json
import sys
from pathlib import Path

import pandas as pd
import sklearn
from sklearn.feature_extraction.text import CountVectorizer
from sklearn.metrics import make_scorer
from sklearn.metrics import precision_score, recall_score
from sklearn.model_selection import (
    StratifiedKFold,
    cross_val_score,
    cross_validate,
)
from sklearn.naive_bayes import MultinomialNB
from sklearn.pipeline import Pipeline


sys.stdout = io.TextIOWrapper(
    sys.stdout.buffer,
    encoding="utf-8"
)


BASE_DIR = Path(__file__).resolve().parent
FILES_DIR = BASE_DIR.parent / "files"

RUTA_TRAIN = FILES_DIR / "dataset_237_train.csv"
RUTA_RESULTADO = FILES_DIR / "resultado_baseline_237.json"
RUTA_FOLDS = FILES_DIR / "resultados_folds_baseline_237.csv"

SEMILLA = 42
N_FOLDS = 5


def hash_archivo(ruta):
    sha256 = hashlib.sha256()

    with open(ruta, "rb") as archivo:
        while True:
            bloque = archivo.read(1024 * 1024)

            if not bloque:
                break

            sha256.update(bloque)

    return sha256.hexdigest()


def validar_train(df):
    columnas = [
        "documento_id",
        "texto",
        "etiqueta"
    ]

    if list(df.columns) != columnas:
        raise ValueError(
            f"Columnas inesperadas en TRAIN: {list(df.columns)}"
        )

    df["documento_id"] = (
        df["documento_id"]
        .astype(str)
        .str.strip()
    )
    df["etiqueta"] = (
        df["etiqueta"]
        .astype(str)
        .str.strip()
        .str.lower()
    )

    if len(df) != 189:
        raise ValueError(
            f"TRAIN debería tener 189 documentos. Tiene {len(df)}."
        )

    if df["documento_id"].duplicated().any():
        raise ValueError("TRAIN contiene documento_id duplicados.")

    if (
        df["texto"]
        .fillna("")
        .astype(str)
        .str.strip()
        .eq("")
        .any()
    ):
        raise ValueError("TRAIN contiene textos vacíos.")

    conteo = df["etiqueta"].value_counts().to_dict()

    if conteo.get("civil", 0) != 90:
        raise ValueError("TRAIN no contiene 90 documentos Civil.")

    if conteo.get("penal", 0) != 99:
        raise ValueError("TRAIN no contiene 99 documentos Penal.")

    return conteo


def crear_pipeline_baseline():
    return Pipeline([
        (
            "vectorizador",
            CountVectorizer(
                lowercase=True,
                max_features=None,
                ngram_range=(1, 1),
                min_df=1,
                stop_words=None
            )
        ),
        (
            "clasificador",
            MultinomialNB(
                alpha=1.0
            )
        )
    ])


def main():
    print("Cargando TRAIN para baseline...")

    train = pd.read_csv(
        RUTA_TRAIN,
        dtype={
            "documento_id": str
        }
    )

    conteo = validar_train(train)

    X = train["texto"].astype(str).to_numpy()
    y = train["etiqueta"].astype(str).to_numpy()

    cv = StratifiedKFold(
        n_splits=N_FOLDS,
        shuffle=True,
        random_state=SEMILLA
    )

    f1_macro_folds = cross_val_score(
        crear_pipeline_baseline(),
        X,
        y,
        cv=cv,
        scoring="f1_macro",
        n_jobs=1
    )

    metricas = {
        "accuracy": "accuracy",
        "precision_macro": make_scorer(
            precision_score,
            average="macro",
            zero_division=0
        ),
        "recall_macro": make_scorer(
            recall_score,
            average="macro",
            zero_division=0
        )
    }

    resultados = cross_validate(
        crear_pipeline_baseline(),
        X,
        y,
        cv=cv,
        scoring=metricas,
        return_train_score=False,
        n_jobs=1
    )

    filas = []

    for indice in range(N_FOLDS):
        filas.append({
            "fold": indice + 1,
            "accuracy": resultados["test_accuracy"][indice],
            "precision_macro": resultados[
                "test_precision_macro"
            ][indice],
            "recall_macro": resultados[
                "test_recall_macro"
            ][indice],
            "f1_macro": f1_macro_folds[indice]
        })

    folds = pd.DataFrame(filas)

    resumen = {
        "etapa": "C1_baseline",
        "dataset": str(RUTA_TRAIN),
        "hash_train": hash_archivo(RUTA_TRAIN),
        "total_train": int(len(train)),
        "train_civil": int(conteo.get("civil", 0)),
        "train_penal": int(conteo.get("penal", 0)),
        "semilla": SEMILLA,
        "n_folds": N_FOLDS,
        "validacion_cruzada": {
            "estrategia": "StratifiedKFold",
            "shuffle": True,
            "metodo_f1_macro": "cross_val_score",
            "metodo_metricas_complementarias": "cross_validate"
        },
        "pipeline": {
            "vectorizador": {
                "tipo": "CountVectorizer",
                "lowercase": True,
                "max_features": None,
                "ngram_range": [1, 1],
                "min_df": 1,
                "stop_words": None
            },
            "clasificador": {
                "tipo": "MultinomialNB",
                "alpha": 1.0
            }
        },
        "metricas": {
            "accuracy_media": float(folds["accuracy"].mean()),
            "accuracy_std": float(folds["accuracy"].std(ddof=1)),
            "precision_macro_media": float(
                folds["precision_macro"].mean()
            ),
            "precision_macro_std": float(
                folds["precision_macro"].std(ddof=1)
            ),
            "recall_macro_media": float(
                folds["recall_macro"].mean()
            ),
            "recall_macro_std": float(
                folds["recall_macro"].std(ddof=1)
            ),
            "f1_macro_media": float(folds["f1_macro"].mean()),
            "f1_macro_std": float(folds["f1_macro"].std(ddof=1))
        },
        "versiones": {
            "python": sys.version,
            "scikit_learn": sklearn.__version__,
            "pandas": pd.__version__
        }
    }

    filas_resumen = [
        {
            "fold": "Promedio",
            "accuracy": folds["accuracy"].mean(),
            "precision_macro": folds["precision_macro"].mean(),
            "recall_macro": folds["recall_macro"].mean(),
            "f1_macro": folds["f1_macro"].mean()
        },
        {
            "fold": "Desviación estándar",
            "accuracy": folds["accuracy"].std(ddof=1),
            "precision_macro": folds["precision_macro"].std(ddof=1),
            "recall_macro": folds["recall_macro"].std(ddof=1),
            "f1_macro": folds["f1_macro"].std(ddof=1)
        }
    ]

    folds_salida = pd.concat(
        [
            folds,
            pd.DataFrame(filas_resumen)
        ],
        ignore_index=True
    )

    folds_salida.to_csv(
        RUTA_FOLDS,
        index=False,
        encoding="utf-8-sig"
    )

    with open(
        RUTA_RESULTADO,
        "w",
        encoding="utf-8"
    ) as archivo:
        json.dump(
            resumen,
            archivo,
            indent=4,
            ensure_ascii=False
        )

    print("=" * 70)
    print("C1 BASELINE FINALIZADO")
    print("=" * 70)
    print("TRAIN:", len(train))
    print("Civil:", conteo.get("civil", 0))
    print("Penal:", conteo.get("penal", 0))
    print(
        "\nF1 macro CV (media +/- desviación estándar):",
        f"{resumen['metricas']['f1_macro_media']:.6f} +/- "
        f"{resumen['metricas']['f1_macro_std']:.6f}"
    )
    print("\nArchivos generados:")
    print(RUTA_RESULTADO)
    print(RUTA_FOLDS)


if __name__ == "__main__":
    main()
