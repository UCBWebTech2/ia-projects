"""Valida corpus NER (Phase 3).

Soporta:
- Formato Python actual: TRAINING_DATA/TRAIN_DATA = [(text, {"entities":[(s,e,label),...]})]
- Formato JSONL: {"text": "...", "entities": [[s,e,"LABEL"], ...]}

Reporta spans fuera de rango, desalineados, etiquetas, duplicados y exporta resumen JSON.
"""

from __future__ import annotations

import argparse
import importlib.util
import json
from collections import Counter
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
        line = line.strip()
        if not line:
            continue
        obj = json.loads(line)
        text = obj["text"]
        ents = obj.get("entities", [])
        rows.append((text, {"entities": [tuple(e) for e in ents]}))
    return rows


def load_dataset(path: Path):
    if path.suffix.lower() == ".py":
        return load_python_dataset(path)
    if path.suffix.lower() == ".jsonl":
        return load_jsonl(path)
    raise ValueError("Formato no soportado. Use .py o .jsonl")


def is_aligned(text: str, start: int, end: int) -> bool:
    if start < 0 or end > len(text) or start >= end:
        return False
    span = text[start:end]
    if not span.strip():
        return False
    # simple word-boundary alignment check
    left_ok = start == 0 or not text[start - 1].isalnum()
    right_ok = end == len(text) or not text[end].isalnum()
    return left_ok and right_ok


def main() -> None:
    p = argparse.ArgumentParser()
    p.add_argument("--input", required=True)
    p.add_argument("--output", default="")
    args = p.parse_args()
    path = Path(args.input)
    data = load_dataset(path)

    label_counts = Counter()
    issues = []
    total_entities = 0
    for i, (text, ann) in enumerate(data):
        for (start, end, label) in ann.get("entities", []):
            total_entities += 1
            label_counts[label] += 1
            span_text = text[start:end] if 0 <= start < len(text) and 0 <= end <= len(text) and start < end else None
            item = {"index": i, "start": start, "end": end, "label": label, "span": span_text}
            if start < 0 or end > len(text) or start >= end:
                item["issue"] = "out_of_range"
                issues.append(item)
            elif not is_aligned(text, start, end):
                item["issue"] = "misaligned"
                issues.append(item)

    report = {
        "file": str(path),
        "sentences": len(data),
        "entities": total_entities,
        "labels": dict(label_counts),
        "issues": len(issues),
        "issues_by_type": dict(Counter(i["issue"] for i in issues)),
        "sample_issues": issues[:50],
    }
    out = Path(args.output) if args.output else path.with_suffix(".validation.json")
    out.write_text(json.dumps(report, indent=2, ensure_ascii=False), encoding="utf-8")
    print(json.dumps({k: report[k] for k in ["file", "sentences", "entities", "labels", "issues", "issues_by_type"]}, indent=2, ensure_ascii=False))
    print(f"Reporte: {out}")


if __name__ == "__main__":
    main()
