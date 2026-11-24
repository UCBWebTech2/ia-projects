"""Reproducible training pipeline for the Civil/Penal classifier (Phase 1).

Generates a sklearn Pipeline(CountVectorizer, MultinomialNB), evaluates with
stratified holdout + optional cross-validation, and writes model + metadata.

Usage examples:
  python OCR_API/scripts/entrenar_v2.py
  python OCR_API/scripts/entrenar_v2.py --train OCR_API/files/train_4000.csv --test OCR_API/files/test_4000.csv
  python OCR_API/scripts/entrenar_v2.py --cross-validate --cv-folds 5
  python OCR_API/scripts/entrenar_v2.py --grid-search
"""

from __future__ import annotations

import argparse
import hashlib
import json
import platform
import sys
from datetime import datetime, timezone
from pathlib import Path

import joblib
import pandas as pd
from sklearn.feature_extraction.text import CountVectorizer
from sklearn.metrics import (
    accuracy_score,
    classification_report,
    confusion_matrix,
    f1_score,
)
from sklearn.model_selection import GridSearchCV, StratifiedKFold, cross_validate, train_test_split
from sklearn.naive_bayes import MultinomialNB
from sklearn.pipeline import Pipeline

ROOT = Path(__file__).resolve().parents[2]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from OCR_API.app.core.config import settings  # noqa: E402

ALLOWED_LABELS = {"civil", "penal"}
DEFAULT_PARAMS = {
    "vectorizer__max_features": 10000,
    "vectorizer__min_df": 2,
    "vectorizer__ngram_range": (1, 2),
    "classifier__alpha": 0.5,
}


def sha256_file(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def sha256_text(text: str) -> str:
    return hashlib.sha256(text.encode("utf-8")).hexdigest()


def normalize_text(value: object) -> str:
    return " ".join(str(value).split()).strip()


def load_dataset(path: Path) -> pd.DataFrame:
    df = pd.read_csv(path)
    if "texto" not in df.columns or "etiqueta" not in df.columns:
        raise ValueError(f"{path}: debe contener columnas 'texto' y 'etiqueta'")
    df = df[["texto", "etiqueta"]].copy()
    df["texto"] = df["texto"].fillna("").map(normalize_text)
    df["etiqueta"] = df["etiqueta"].fillna("").map(lambda x: normalize_text(x).lower())
    before = len(df)
    df = df[df["texto"] != ""]
    df = df[df["etiqueta"].isin(ALLOWED_LABELS)]
    df = df.drop_duplicates(subset=["texto", "etiqueta"])
    dropped = before - len(df)
    if dropped:
        print(f"⚠️ {path.name}: {dropped} filas descartadas por validación/dedupe")
    if df.empty:
        raise ValueError(f"{path}: no quedan filas válidas tras validación")
    return df.reset_index(drop=True)


def build_pipeline(params: dict | None = None) -> Pipeline:
    pipe = Pipeline(
        [
            (
                "vectorizer",
                CountVectorizer(
                    max_features=10000,
                    min_df=2,
                    ngram_range=(1, 2),
                ),
            ),
            ("classifier", MultinomialNB(alpha=0.5)),
        ]
    )
    pipe.set_params(**(params or DEFAULT_PARAMS))
    return pipe


def evaluate(model: Pipeline, X, y) -> dict:
    pred = model.predict(X)
    labels = sorted(ALLOWED_LABELS)
    report = classification_report(y, pred, labels=labels, output_dict=True, zero_division=0)
    cm = confusion_matrix(y, pred, labels=labels)
    return {
        "accuracy": float(accuracy_score(y, pred)),
        "macro_f1": float(f1_score(y, pred, average="macro", zero_division=0)),
        "weighted_f1": float(f1_score(y, pred, average="weighted", zero_division=0)),
        "labels": labels,
        "confusion_matrix": cm.tolist(),
        "classification_report": report,
    }


def maybe_grid_search(pipe: Pipeline, X, y, cv_folds: int) -> tuple[Pipeline, dict | None]:
    grid = {
        "vectorizer__max_features": [5000, 10000],
        "vectorizer__min_df": [1, 2],
        "vectorizer__ngram_range": [(1, 1), (1, 2)],
        "classifier__alpha": [0.1, 0.5, 1.0],
    }
    cv = StratifiedKFold(n_splits=cv_folds, shuffle=True, random_state=42)
    search = GridSearchCV(pipe, grid, cv=cv, scoring="f1_macro", n_jobs=1, verbose=0)
    search.fit(X, y)
    return search.best_estimator_, {
        "best_params": search.best_params_,
        "best_score_macro_f1": float(search.best_score_),
        "cv_folds": cv_folds,
    }


def main() -> None:
    parser = argparse.ArgumentParser(description="Entrenamiento reproducible Civil/Penal")
    parser.add_argument("--train", default=str(settings.files_dir / "train_4000.csv"))
    parser.add_argument("--test", default=str(settings.files_dir / "test_4000.csv"))
    parser.add_argument("--output-model", default=str(settings.civil_penal_model_path))
    parser.add_argument("--metadata-out", default="")
    parser.add_argument("--cross-validate", action="store_true")
    parser.add_argument("--cv-folds", type=int, default=5)
    parser.add_argument("--grid-search", action="store_true")
    parser.add_argument("--test-size", type=float, default=0.2, help="Usado solo si no hay --test")
    parser.add_argument("--min-delta-macro-f1", type=float, default=0.0)
    parser.add_argument("--force", action="store_true", help="Ignora gate de aceptación")
    parser.add_argument("--no-save", action="store_true")
    args = parser.parse_args()

    train_path = Path(args.train).resolve()
    test_path = Path(args.test).resolve() if args.test else None
    output_model = Path(args.output_model).resolve()
    metadata_out = Path(args.metadata_out).resolve() if args.metadata_out else output_model.with_suffix(".metadata.json")

    train_df = load_dataset(train_path)
    if test_path and test_path.exists():
        test_df = load_dataset(test_path)
        X_train, y_train = train_df["texto"], train_df["etiqueta"]
        X_test, y_test = test_df["texto"], test_df["etiqueta"]
        split_info = {"type": "explicit_files", "train": str(train_path), "test": str(test_path)}
    else:
        X_train, X_test, y_train, y_test = train_test_split(
            train_df["texto"], train_df["etiqueta"], test_size=args.test_size, stratify=train_df["etiqueta"], random_state=42
        )
        split_info = {"type": "stratified_split", "train": str(train_path), "test_size": args.test_size, "random_state": 42}
        test_df = pd.DataFrame({"texto": X_test, "etiqueta": y_test})

    print(f"Train: {len(X_train)} | Test: {len(X_test)}")
    print("Distribución train:", y_train.value_counts().to_dict())
    print("Distribución test:", y_test.value_counts().to_dict())

    pipe = build_pipeline(DEFAULT_PARAMS)
    grid_info = None
    if args.grid_search:
        print("Ejecutando GridSearchCV...")
        pipe, grid_info = maybe_grid_search(pipe, X_train, y_train)
        print("Mejores params:", grid_info["best_params"])

    pipe.fit(X_train, y_train)
    metrics = evaluate(pipe, X_test, y_test)

    cv_info = None
    if args.cross_validate:
        cv = StratifiedKFold(n_splits=args.cv_folds, shuffle=True, random_state=42)
        cv_res = cross_validate(pipe, X_train, y_train, cv=cv, scoring=["accuracy", "f1_macro", "f1_weighted"], n_jobs=1)
        cv_info = {
            "folds": args.cv_folds,
            "accuracy_mean": float(cv_res["test_accuracy"].mean()),
            "accuracy_std": float(cv_res["test_accuracy"].std()),
            "macro_f1_mean": float(cv_res["test_f1_macro"].mean()),
            "macro_f1_std": float(cv_res["test_f1_macro"].std()),
            "weighted_f1_mean": float(cv_res["test_f1_weighted"].mean()),
            "weighted_f1_std": float(cv_res["test_f1_weighted"].std()),
        }

    print("Accuracy:", round(metrics["accuracy"], 4))
    print("Macro F1:", round(metrics["macro_f1"], 4))
    print("Matriz de confusión [civil, penal]:", metrics["confusion_matrix"])

    metadata = {
        "created_at_utc": datetime.now(timezone.utc).isoformat(),
        "python": platform.python_version(),
        "platform": platform.platform(),
        "script": "OCR_API/scripts/entrenar_v2.py",
        "model_output": str(output_model),
        "pipeline": {
            "type": "sklearn.pipeline.Pipeline",
            "steps": ["CountVectorizer", "MultinomialNB"],
            "params": {
                "vectorizer__max_features": pipe.named_steps["vectorizer"].max_features,
                "vectorizer__min_df": pipe.named_steps["vectorizer"].min_df,
                "vectorizer__ngram_range": list(pipe.named_steps["vectorizer"].ngram_range),
                "classifier__alpha": pipe.named_steps["classifier"].alpha,
            },
        },
        "split": split_info,
        "datasets": {
            "train": {
                "path": str(train_path),
                "sha256": sha256_file(train_path) if train_path.exists() else None,
                "rows": int(len(train_df)),
                "labels": train_df["etiqueta"].value_counts().to_dict(),
            },
            "test": {
                "path": str(test_path) if test_path else None,
                "sha256": sha256_file(test_path) if test_path and test_path.exists() else None,
                "rows": int(len(test_df)),
                "labels": test_df["etiqueta"].value_counts().to_dict(),
            },
        },
        "metrics": metrics,
        "cross_validation": cv_info,
        "grid_search": grid_info,
        "notes": [
            "Pipeline reproducible Fase 1. No incluye corpus original de 6400 si no está en repo.",
            "Las métricas deben compararse contra test congelado con hash.",
        ],
    }

    # Phase 1/2 gate: compare against previous metadata if present.
    previous_macro_f1 = None
    if metadata_out.exists():
        try:
            prev = json.loads(metadata_out.read_text(encoding="utf-8"))
            previous_macro_f1 = float(prev.get("metrics", {}).get("macro_f1"))
        except Exception:
            previous_macro_f1 = None
    metadata["acceptance_gate"] = {
        "previous_macro_f1": previous_macro_f1,
        "candidate_macro_f1": metrics["macro_f1"],
        "min_delta_macro_f1": args.min_delta_macro_f1,
        "forced": bool(args.force),
        "accepted": True,
    }
    if previous_macro_f1 is not None and not args.force:
        delta = metrics["macro_f1"] - previous_macro_f1
        metadata["acceptance_gate"]["delta_macro_f1"] = delta
        if delta < args.min_delta_macro_f1:
            metadata["acceptance_gate"]["accepted"] = False
            print(f"❌ Gate rechazado: delta macro_f1 {delta:.4f} < {args.min_delta_macro_f1:.4f}")
            if not args.no_save:
                rejected_path = metadata_out.with_suffix(".rejected.json")
                rejected_path.write_text(json.dumps(metadata, indent=2, ensure_ascii=False), encoding="utf-8")
                print(f"Metadata de rechazo: {rejected_path}")
            raise SystemExit(2)

    if not args.no_save:
        output_model.parent.mkdir(parents=True, exist_ok=True)
        joblib.dump(pipe, output_model)
        metadata_out.parent.mkdir(parents=True, exist_ok=True)
        metadata_out.write_text(json.dumps(metadata, indent=2, ensure_ascii=False), encoding="utf-8")
        print(f"✅ Modelo guardado: {output_model}")
        print(f"✅ Metadata guardada: {metadata_out}")
    else:
        print(json.dumps(metadata, indent=2, ensure_ascii=False))


if __name__ == "__main__":
    main()
