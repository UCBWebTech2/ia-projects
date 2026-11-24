"""Evaluación baseline NER (Phase 3 / R-14).

Uso:
  python OCR_API/scripts/evaluar_ner.py --data OCR_API/files/training_ner_data.py
  python OCR_API/scripts/evaluar_ner.py --data OCR_API/files/ner_dataset.jsonl --model es_core_news_lg

Salida: métricas globales y por etiqueta (ents_p/r/f) en JSON.
"""

from __future__ import annotations

import argparse
import importlib.util
import json
from pathlib import Path


def load_python_dataset(path: Path):
    spec = importlib.util.spec_from_file_location("ner_data", path)
    mod = importlib.util.module_from_spec(spec)
    assert spec and spec.loader
    spec.loader.exec_module(mod)
    data = getattr(mod, "TRAINING_DATA", None) or getattr(mod, "TRAIN_DATA", None)
    if data is None:
        raise ValueError("No se encontró TRAINING_DATA/TRAIN_DATA")
    return data


def load_jsonl(path: Path):
    rows = []
    for line in path.read_text(encoding="utf-8").splitlines():
        if not line.strip():
            continue
        obj = json.loads(line)
        rows.append((obj["text"], {"entities": [tuple(e) for e in obj.get("entities", [])]}))
    return rows


def load_dataset(path: Path):
    if path.suffix == ".py":
        return load_python_dataset(path)
    if path.suffix == ".jsonl":
        return load_jsonl(path)
    raise ValueError("Formato no soportado (.py/.jsonl)")


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("--data", required=True)
    ap.add_argument("--model", default="es_core_news_lg")
    ap.add_argument("--output", default="")
    ap.add_argument("--use-entity-ruler", action="store_true", help="Usa OCR_API.scripts.EntityRuler.nlp")
    args = ap.parse_args()

    try:
        import spacy
        from spacy.scorer import Scorer
        from spacy.training import Example
    except Exception as e:
        raise SystemExit(f"spaCy no disponible: {e}")

    data = load_dataset(Path(args.data))
    if args.use_entity_ruler:
        from OCR_API.scripts.EntityRuler import nlp
    else:
        nlp = spacy.load(args.model)

    examples = []
    skipped = 0
    for text, ann in data:
        try:
            pred = nlp(text)
            examples.append(Example.from_dict(pred, ann))
        except Exception:
            skipped += 1

    scorer = Scorer()
    scores = scorer.score(examples)
    # Normalize for JSON
    def norm(x):
        if isinstance(x, dict):
            return {k: norm(v) for k, v in x.items()}
        if isinstance(x, (list, tuple)):
            return [norm(v) for v in x]
        if hasattr(x, "item"):
            try:
                return x.item()
            except Exception:
                pass
        return x

    report = {
        "data": args.data,
        "model": args.model,
        "use_entity_ruler": args.use_entity_ruler,
        "examples": len(examples),
        "skipped": skipped,
        "ents_p": scores.get("ents_p"),
        "ents_r": scores.get("ents_r"),
        "ents_f": scores.get("ents_f"),
        "ents_per_type": norm(scores.get("ents_per_type", {})),
    }
    out = Path(args.output) if args.output else Path(args.data).with_suffix(".ner_eval.json")
    out.write_text(json.dumps(report, indent=2, ensure_ascii=False), encoding="utf-8")
    print(json.dumps(report, indent=2, ensure_ascii=False))
    print(f"Reporte: {out}")


if __name__ == "__main__":
    main()
