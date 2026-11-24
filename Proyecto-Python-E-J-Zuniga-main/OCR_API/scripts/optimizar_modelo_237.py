import hashlib
import io
import json
import sys
from pathlib import Path

import pandas as pd
import sklearn
from sklearn.feature_extraction.text import CountVectorizer
from sklearn.model_selection import GridSearchCV, StratifiedKFold
from sklearn.naive_bayes import MultinomialNB
from sklearn.pipeline import Pipeline


sys.stdout = io.TextIOWrapper(
    sys.stdout.buffer,
    encoding="utf-8"
)


BASE_DIR = Path(__file__).resolve().parent
FILES_DIR = BASE_DIR.parent / "files"

RUTA_TRAIN = FILES_DIR / "dataset_237_train.csv"
RUTA_GRID = FILES_DIR / "resultados_gridsearch_237.csv"
RUTA_RESUMEN = FILES_DIR / "resumen_gridsearch_237.json"
RUTA_CONFIG = FILES_DIR / "configuracion_seleccionada_237.json"
RUTA_BASELINE = FILES_DIR / "resultado_baseline_237.json"

SEMILLA = 42
N_FOLDS = 5
EPSILON_EMPATE = 0.001


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


def crear_pipeline():
    return Pipeline([
        (
            "vectorizador",
            CountVectorizer(
                lowercase=True,
                stop_words=None
            )
        ),
        (
            "clasificador",
            MultinomialNB()
        )
    ])


def param_grid():
    return {
        "vectorizador__max_features": [
            3000,
            5000,
            10000,
            None
        ],
        "vectorizador__ngram_range": [
            (1, 1),
            (1, 2)
        ],
        "vectorizador__min_df": [
            1,
            2
        ],
        "clasificador__alpha": [
            0.1,
            0.5,
            1.0
        ]
    }


def valor_parametro(row, columna):
    valor = row[columna]

    if pd.isna(valor):
        return None

    return valor


def simplicidad(row):
    ngram = row["param_ngram_range"]
    max_features = row["param_max_features"]
    min_df = row["param_min_df"]
    alpha = row["param_alpha"]

    ngram_score = 0 if ngram == "(1, 1)" else 1

    max_features_orden = {
        3000: 0,
        5000: 1,
        10000: 2,
        None: 3,
        "None": 3
    }
    max_features_score = max_features_orden.get(
        max_features,
        3
    )

    min_df_score = 0 if int(min_df) == 2 else 1
    alpha_score = abs(float(alpha) - 1.0)

    return (
        ngram_score,
        max_features_score,
        min_df_score,
        alpha_score
    )


def seleccionar_configuracion(resultados):
    mejor_f1 = float(resultados["mean_test_score"].max())

    candidatos = resultados[
        resultados["mean_test_score"] >= (
            mejor_f1 - EPSILON_EMPATE
        )
    ].copy()

    candidatos["simplicidad"] = candidatos.apply(
        simplicidad,
        axis=1
    )

    candidatos = candidatos.sort_values(
        by=[
            "std_test_score",
            "simplicidad",
            "rank_test_score"
        ],
        ascending=[
            True,
            True,
            True
        ]
    )

    return candidatos.iloc[0], mejor_f1, candidatos


def convertir_ngram(valor):
    if valor == "(1, 1)":
        return [1, 1]

    if valor == "(1, 2)":
        return [1, 2]

    raise ValueError(
        f"ngram_range inesperado: {valor}"
    )


def main():
    print("Cargando TRAIN para GridSearchCV...")

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

    grid = GridSearchCV(
        estimator=crear_pipeline(),
        param_grid=param_grid(),
        scoring="f1_macro",
        cv=cv,
        n_jobs=-1,
        return_train_score=True,
        refit=False,
        verbose=2,
        error_score="raise"
    )

    grid.fit(X, y)

    resultados = pd.DataFrame(grid.cv_results_)

    salida = pd.DataFrame({
        "param_max_features": resultados[
            "param_vectorizador__max_features"
        ].where(
            resultados["param_vectorizador__max_features"].notna(),
            None
        ),
        "param_ngram_range": resultados[
            "param_vectorizador__ngram_range"
        ].astype(str),
        "param_min_df": resultados[
            "param_vectorizador__min_df"
        ].astype(int),
        "param_alpha": resultados[
            "param_clasificador__alpha"
        ].astype(float),
        "mean_test_score": resultados["mean_test_score"],
        "std_test_score": resultados["std_test_score"],
        "mean_train_score": resultados["mean_train_score"],
        "std_train_score": resultados["std_train_score"],
        "rank_test_score": resultados["rank_test_score"]
    })

    salida = salida.sort_values(
        [
            "rank_test_score",
            "std_test_score"
        ]
    ).reset_index(drop=True)

    seleccionada, mejor_f1, candidatos_empate = (
        seleccionar_configuracion(salida)
    )

    baseline_grid = salida[
        salida["param_max_features"].isna()
        & (salida["param_ngram_range"] == "(1, 1)")
        & (salida["param_min_df"] == 1)
        & (salida["param_alpha"] == 1.0)
    ]

    baseline_cv = None
    diferencia_baseline = None

    if RUTA_BASELINE.exists() and not baseline_grid.empty:
        with open(
            RUTA_BASELINE,
            "r",
            encoding="utf-8"
        ) as archivo:
            baseline = json.load(archivo)

        baseline_cv = baseline["metricas"]["f1_macro_media"]
        diferencia_baseline = abs(
            float(baseline_cv)
            - float(baseline_grid.iloc[0]["mean_test_score"])
        )

    config = {
        "etapa": "C3_seleccion_configuracion",
        "criterio": "f1_macro",
        "regla_desempate": [
            "Mayor mean F1 macro de validación cruzada.",
            "Si la diferencia es menor a 0.001, menor desviación estándar.",
            "Si continúa el empate, configuración más simple."
        ],
        "epsilon_empate": EPSILON_EMPATE,
        "max_features": (
            None
            if pd.isna(seleccionada["param_max_features"])
            else int(seleccionada["param_max_features"])
        ),
        "ngram_range": convertir_ngram(
            seleccionada["param_ngram_range"]
        ),
        "min_df": int(seleccionada["param_min_df"]),
        "alpha": float(seleccionada["param_alpha"]),
        "cv_f1_macro_media": float(
            seleccionada["mean_test_score"]
        ),
        "cv_f1_macro_std": float(
            seleccionada["std_test_score"]
        ),
        "cv_train_f1_macro_media": float(
            seleccionada["mean_train_score"]
        ),
        "cv_train_f1_macro_std": float(
            seleccionada["std_train_score"]
        ),
        "rank_test_score_original": int(
            seleccionada["rank_test_score"]
        ),
        "mejor_f1_macro_observado": mejor_f1,
        "configuraciones_evaluadas": int(len(salida)),
        "ajustes_totales": int(len(salida) * N_FOLDS),
        "candidatos_en_empate_tecnico": int(
            len(candidatos_empate)
        ),
        "baseline_control": {
            "incluido_en_grid": bool(not baseline_grid.empty),
            "f1_macro_baseline_script": (
                None
                if baseline_cv is None
                else float(baseline_cv)
            ),
            "f1_macro_baseline_grid": (
                None
                if baseline_grid.empty
                else float(
                    baseline_grid.iloc[0]["mean_test_score"]
                )
            ),
            "diferencia_absoluta": (
                None
                if diferencia_baseline is None
                else float(diferencia_baseline)
            )
        }
    }

    resumen = {
        "etapa": "C2_gridsearch",
        "dataset": str(RUTA_TRAIN),
        "hash_train": hash_archivo(RUTA_TRAIN),
        "total_train": int(len(train)),
        "train_civil": int(conteo.get("civil", 0)),
        "train_penal": int(conteo.get("penal", 0)),
        "semilla": SEMILLA,
        "n_folds": N_FOLDS,
        "scoring": "f1_macro",
        "validacion_cruzada": {
            "metodo": "GridSearchCV",
            "estrategia": "StratifiedKFold",
            "shuffle": True
        },
        "refit": False,
        "configuraciones_evaluadas": int(len(salida)),
        "ajustes_totales": int(len(salida) * N_FOLDS),
        "mejor_f1_macro_observado": mejor_f1,
        "configuracion_seleccionada": config,
        "versiones": {
            "python": sys.version,
            "scikit_learn": sklearn.__version__,
            "pandas": pd.__version__
        }
    }

    salida.to_csv(
        RUTA_GRID,
        index=False,
        encoding="utf-8-sig"
    )

    with open(
        RUTA_RESUMEN,
        "w",
        encoding="utf-8"
    ) as archivo:
        json.dump(
            resumen,
            archivo,
            indent=4,
            ensure_ascii=False
        )

    with open(
        RUTA_CONFIG,
        "w",
        encoding="utf-8"
    ) as archivo:
        json.dump(
            config,
            archivo,
            indent=4,
            ensure_ascii=False
        )

    print("=" * 70)
    print("C2 GRIDSEARCH + C3 SELECCIÓN FINALIZADOS")
    print("=" * 70)
    print("Configuraciones evaluadas:", len(salida))
    print("Ajustes totales:", len(salida) * N_FOLDS)
    print("\nConfiguración seleccionada:")
    print(json.dumps(config, indent=4, ensure_ascii=False))
    print(
        "\nF1 macro CV (media +/- desviación estándar):",
        f"{config['cv_f1_macro_media']:.6f} +/- "
        f"{config['cv_f1_macro_std']:.6f}"
    )
    print("\nArchivos generados:")
    print(RUTA_GRID)
    print(RUTA_RESUMEN)
    print(RUTA_CONFIG)


if __name__ == "__main__":
    main()
