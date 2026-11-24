"""Crea partición 70/15/15 estratificada simple por archivo para corpus NER JSONL.

Uso:
  python OCR_API/scripts/split_ner_corpus.py --input OCR_API/files/ner_annotation/corpus.jsonl --out-dir OCR_API/files/ner_annotation/splits
"""
from __future__ import annotations
import argparse, json, random
from pathlib import Path

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--input', required=True)
    ap.add_argument('--out-dir', required=True)
    ap.add_argument('--seed', type=int, default=42)
    args = ap.parse_args()
    rows = [json.loads(l) for l in Path(args.input).read_text(encoding='utf-8').splitlines() if l.strip()]
    random.Random(args.seed).shuffle(rows)
    n = len(rows)
    n_train = int(n * 0.70)
    n_dev = int(n * 0.15)
    splits = {
        'train.jsonl': rows[:n_train],
        'dev.jsonl': rows[n_train:n_train+n_dev],
        'test.jsonl': rows[n_train+n_dev:],
    }
    out = Path(args.out_dir); out.mkdir(parents=True, exist_ok=True)
    for name, items in splits.items():
        (out/name).write_text('\n'.join(json.dumps(x, ensure_ascii=False) for x in items), encoding='utf-8')
    print({k: len(v) for k,v in splits.items()})

if __name__ == '__main__':
    main()
