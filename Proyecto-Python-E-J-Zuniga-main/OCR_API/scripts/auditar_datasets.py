"""Audita datasets de clasificación: esquema, nulos, duplicados, solapamiento, hashes.

Uso:
  python OCR_API/scripts/auditar_datasets.py
  python OCR_API/scripts/auditar_datasets.py --write-manifest OCR_API/files/dataset_manifest.json
"""

from __future__ import annotations

import argparse
import hashlib
import json
from datetime import datetime, timezone
from pathlib import Path

import pandas as pd

ROOT = Path(__file__).resolve().parents[2]
FILES = ROOT / "OCR_API" / "files"
DEFAULT_FILES = [
    FILES / "train_4000.csv",
    FILES / "test_4000.csv",
    FILES / "test_dataset.csv",
    FILES / "dataset_derecho.csv",
    FILES / "dataset_derecho_4k.csv",
]
ALLOWED = {"civil", "penal"}


def sha256_file(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def row_hash(text: str, label: str) -> str:
    norm = " ".join(str(text).split()).strip()
    lab = " ".join(str(label).split()).strip().lower()
    return hashlib.sha256(f"{lab}\n{norm}".encode("utf-8")).hexdigest()


def audit_file(path: Path) -> dict:
    info = {"path": str(path), "exists": path.exists()}
    if not path.exists():
        return info
    df = pd.read_csv(path)
    info["sha256"] = sha256_file(path)
    info["rows"] = int(len(df))
    info["columns"] = list(df.columns)
    if "texto" in df.columns and "etiqueta" in df.columns:
        text = df["texto"].fillna("").astype(str).map(lambda x: " ".join(x.split()).strip())
        label = df["etiqueta"].fillna("").astype(str).map(lambda x: " ".join(x.split()).strip().lower())
        hashes = [row_hash(t, l) for t, l in zip(text, label)]
        info["empty_text"] = int((text == "").sum())
        info["invalid_label"] = int((~label.isin(ALLOWED)).sum())
        info["label_counts"] = label.value_counts(dropna=False).to_dict()
        info["duplicate_rows"] = int(len(hashes) - len(set(hashes)))
        info["unique_hashes"] = len(set(hashes))
        info["sample_hashes_head"] = hashes[:3]
    return info


def overlap(a: dict, b: dict) -> int:
    if not a.get("exists") or not b.get("exists"):
        return -1
    da = pd.read_csv(a["path"])
    db = pd.read_csv(b["path"])
    if "texto" not in da.columns or "texto" not in db.columns:
        return -1
    ha = set(row_hash(t, l) for t, l in zip(da["texto"], da.get("etiqueta", "")))
    hb = set(row_hash(t, l) for t, l in zip(db["texto"], db.get("etiqueta", "")))
    return len(ha & hb)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--write-manifest", default=str(FILES / "dataset_manifest.json"))
    args = parser.parse_args()

    reports = [audit_file(p) for p in DEFAULT_FILES]
    pairs = []
    for i in range(len(reports)):
        for j in range(i + 1, len(reports)):
            o = overlap(reports[i], reports[j])
            if o > 0:
                pairs.append({
                    "a": reports[i]["path"], "b": reports[j]["path"], "overlap_rows": o
                })

    manifest = {
        "generated_at_utc": datetime.now(timezone.utc).isoformat(),
        "files": reports,
        "overlaps": pairs,
        "notes": [
            "test_4000.csv debe permanecer congelado para evaluación.",
            "Cualquier fila con etiqueta fuera de {civil, penal} debe corregirse antes de entrenar.",
        ],
    }
    out = Path(args.write_manifest)
    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_text(json.dumps(manifest, indent=2, ensure_ascii=False), encoding="utf-8")
    print(json.dumps({"manifest": str(out), "files": len(reports), "overlaps": pairs}, indent=2, ensure_ascii=False))


if __name__ == "__main__":
    main()
