"""Calcula acuerdo entre dos anotadores sobre JSONL NER.

Compara entidades como spans exactos (start, end, label) por documento.
Reporta:
- kappa global (Cohen) sobre unidades span-nivel por documento
- F1 exact-match por etiqueta y micro
- desacuerdos por documento

Formato esperado por línea:
{"id":"...", "text":"...", "entities":[[s,e,"LABEL"], ...]}
"""
from __future__ import annotations

import argparse
import json
from collections import Counter, defaultdict
from pathlib import Path


def load(path: Path):
    rows = {}
    for line in path.read_text(encoding="utf-8").splitlines():
        if not line.strip():
            continue
        obj = json.loads(line)
        doc_id = str(obj.get("id", obj.get("text")))
        ents = set(tuple(e) for e in obj.get("entities", []))
        rows[doc_id] = {"text": obj.get("text", ""), "entities": ents}
    return rows


def cohen_kappa(y1, y2):
    if not y1:
        return None
    labels = sorted(set(y1) | set(y2))
    if len(labels) == 1:
        return 1.0
    n = len(y1)
    po = sum(1 for a, b in zip(y1, y2) if a == b) / n
    pe = 0.0
    for lab in labels:
        p1 = sum(1 for a in y1 if a == lab) / n
        p2 = sum(1 for b in y2 if b == lab) / n
        pe += p1 * p2
    if pe == 1.0:
        return 1.0
    return (po - pe) / (1 - pe)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--a", required=True)
    ap.add_argument("--b", required=True)
    ap.add_argument("--output", default="")
    args = ap.parse_args()

    A, B = load(Path(args.a)), load(Path(args.b))
    ids = sorted(set(A) & set(B))
    if not ids:
        report = {"ids_compared": 0, "kappa": None, "note": "sin ids comunes"}
        out = Path(args.output) if args.output else Path(args.a).with_suffix(".kappa.json")
        out.write_text(json.dumps(report, indent=2, ensure_ascii=False), encoding="utf-8")
        print(json.dumps(report, ensure_ascii=False))
        return

    # Span-level agreement: for each doc, union of spans, binary presence per annotator.
    y1, y2 = [], []
    tp = fp = fn = 0
    by_label = defaultdict(lambda: {"tp": 0, "fp": 0, "fn": 0})
    disagreements = []

    for doc_id in ids:
        ea, eb = A[doc_id]["entities"], B[doc_id]["entities"]
        union = ea | eb
        for span in sorted(union):
            in_a = span in ea
            in_b = span in eb
            y1.append("Y" if in_a else "N")
            y2.append("Y" if in_b else "N")
            label = span[2]
            if in_a and in_b:
                tp += 1
                by_label[label]["tp"] += 1
            elif in_a and not in_b:
                fn += 1  # relative to B as reference? report symmetrically below
                by_label[label]["fn"] += 1
            elif in_b and not in_a:
                fp += 1
                by_label[label]["fp"] += 1
        if ea != eb:
            disagreements.append({
                "id": doc_id,
                "only_a": sorted(list(ea - eb)),
                "only_b": sorted(list(eb - ea)),
            })

    prec = tp / (tp + fp) if (tp + fp) else 1.0
    rec = tp / (tp + fn) if (tp + fn) else 1.0
    f1 = 2 * prec * rec / (prec + rec) if (prec + rec) else 1.0
    kappa = cohen_kappa(y1, y2)

    label_report = {}
    for lab, c in sorted(by_label.items()):
        p = c["tp"] / (c["tp"] + c["fp"]) if (c["tp"] + c["fp"]) else 1.0
        r = c["tp"] / (c["tp"] + c["fn"]) if (c["tp"] + c["fn"]) else 1.0
        f = 2 * p * r / (p + r) if (p + r) else 1.0
        label_report[lab] = {"tp": c["tp"], "fp": c["fp"], "fn": c["fn"], "precision": p, "recall": r, "f1": f}

    report = {
        "ids_compared": len(ids),
        "span_units": len(y1),
        "kappa": kappa,
        "micro": {"tp": tp, "fp": fp, "fn": fn, "precision": prec, "recall": rec, "f1": f1},
        "by_label": label_report,
        "disagreements": disagreements,
        "note": "Kappa calculado sobre unidades span exacto (start,end,label) del union set por documento; F1 micro trata A como referencia para FP/FN simétricos según solo_a/solo_b.",
    }

    out = Path(args.output) if args.output else Path(args.a).with_suffix(".kappa.json")
    out.write_text(json.dumps(report, indent=2, ensure_ascii=False), encoding="utf-8")
    print(json.dumps({"ids_compared": report["ids_compared"], "kappa": report["kappa"], "f1_micro": report["micro"]["f1"], "disagreements": len(disagreements)}, ensure_ascii=False))


if __name__ == "__main__":
    main()
