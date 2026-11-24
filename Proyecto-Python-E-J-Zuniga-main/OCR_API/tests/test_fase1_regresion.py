import importlib.util
import json
from pathlib import Path

import pandas as pd


ROOT = Path(__file__).resolve().parents[2]


def _load_module(name: str, relative: str):
    path = ROOT / relative
    spec = importlib.util.spec_from_file_location(name, path)
    mod = importlib.util.module_from_spec(spec)
    assert spec and spec.loader
    spec.loader.exec_module(mod)
    return mod


def test_auditar_datasets_row_hash_normalizes():
    mod = _load_module("auditar_datasets", "OCR_API/scripts/auditar_datasets.py")
    a = mod.row_hash("  Hola   mundo  ", " CIVIL ")
    b = mod.row_hash("Hola mundo", "civil")
    assert a == b


def test_entrenar_v2_normalize_and_load(tmp_path):
    mod = _load_module("entrenar_v2", "OCR_API/scripts/entrenar_v2.py")
    csv_path = tmp_path / "data.csv"
    pd.DataFrame(
        {
            "texto": ["  Texto civil  ", "", "Texto penal", "dup", "dup"],
            "etiqueta": ["CIVIL", "civil", "penal", "civil", "civil"],
        }
    ).to_csv(csv_path, index=False)
    df = mod.load_dataset(csv_path)
    assert list(df.columns) == ["texto", "etiqueta"]
    assert set(df["etiqueta"]) <= {"civil", "penal"}
    assert "" not in set(df["texto"])
    assert len(df) == 3  # empty dropped, duplicate dropped


def test_test_4000_freeze_matches_manifest():
    manifest = json.loads((ROOT / "OCR_API/files/dataset_manifest.json").read_text(encoding="utf-8"))
    freeze = json.loads((ROOT / "OCR_API/files/test_4000.freeze.json").read_text(encoding="utf-8"))
    test_entry = next(f for f in manifest["files"] if f["path"].endswith("test_4000.csv"))
    assert freeze["sha256"] == test_entry["sha256"]
    assert freeze["rows"] == test_entry["rows"]


def test_validar_corpus_ner_template():
    mod = _load_module("validar_corpus_ner", "OCR_API/scripts/validar_corpus_ner.py")
    data = mod.load_dataset(ROOT / "OCR_API/files/ner_corpus_template.jsonl")
    assert len(data) == 2
    assert sum(len(x[1]["entities"]) for x in data) == 5
