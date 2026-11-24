"""Genera borrador JSONL limpio desde training_ner_data.py.

- Elimina spans fuera de rango
- Conserva spans válidos (opcionalmente solo alineados con --only-aligned)
- Normaliza etiquetas PERSON->PER, DATE/MISC se conservan pero se marcan
- Añade campos de revisión humana
"""
from __future__ import annotations
import argparse, json, importlib.util
from pathlib import Path

LABEL_MAP = {"PERSON": "PER"}


def load_py(path: Path):
    spec = importlib.util.spec_from_file_location('ner', path)
    mod = importlib.util.module_from_spec(spec); spec.loader.exec_module(mod)
    return getattr(mod, 'TRAINING_DATA', None) or getattr(mod, 'TRAIN_DATA')


def aligned(text, s, e):
    if s < 0 or e > len(text) or s >= e: return False
    if not text[s:e].strip(): return False
    left = s == 0 or not text[s-1].isalnum()
    right = e == len(text) or not text[e].isalnum()
    return left and right


def main():
    ap = argparse.ArgumentParser(); ap.add_argument('--input', required=True); ap.add_argument('--output', required=True)
    ap.add_argument('--only-aligned', action='store_true')
    args = ap.parse_args()
    data = load_py(Path(args.input))
    out=[]
    for i,(text,ann) in enumerate(data):
        ents=[]
        for s,e,l in ann.get('entities',[]):
            l = LABEL_MAP.get(l,l)
            if s < 0 or e > len(text) or s >= e: continue
            if args.only_aligned and not aligned(text,s,e): continue
            ents.append([int(s),int(e),l])
        out.append({
            'id': f'legacy-{i:04d}',
            'text': text,
            'entities': ents,
            'source': 'training_ner_data.py',
            'status': 'DRAFT_NEEDS_HUMAN_REVIEW',
            'annotators': [],
        })
    Path(args.output).write_text('\n'.join(json.dumps(x,ensure_ascii=False) for x in out), encoding='utf-8')
    print(json.dumps({'rows':len(out),'entities':sum(len(x['entities']) for x in out),'output':args.output}, ensure_ascii=False))

if __name__ == '__main__':
    main()
