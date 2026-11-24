"""Reconstruye un corpus con provenance desde datasets disponibles.

No afirma ser el corpus original de 6400; genera un artefacto trazable.
"""
from __future__ import annotations
import argparse, hashlib, json
from pathlib import Path
import pandas as pd

ROOT = Path(__file__).resolve().parents[2]
FILES = ROOT / 'OCR_API' / 'files'
DEFAULT = ['train_4000.csv','test_4000.csv','test_dataset.csv','dataset_derecho.csv','dataset_derecho_4k.csv']


def norm(x):
    return ' '.join(str(x).split()).strip()


def main():
    ap=argparse.ArgumentParser(); ap.add_argument('--out-csv', default=str(FILES/'corpus_reconstruido_provenance.csv'))
    ap.add_argument('--out-json', default=str(FILES/'corpus_reconstruido_provenance.manifest.json'))
    args=ap.parse_args()
    rows=[]; seen=set(); stats=[]
    for name in DEFAULT:
        p=FILES/name
        if not p.exists(): stats.append({'file':name,'exists':False}); continue
        df=pd.read_csv(p)
        added=dup=invalid=0
        for i,r in df.iterrows():
            text=norm(r.get('texto','')); label=norm(r.get('etiqueta','')).lower()
            if not text or label not in {'civil','penal'}: invalid+=1; continue
            h=hashlib.sha256(f'{label}\n{text}'.encode('utf-8')).hexdigest()
            if h in seen: dup+=1; continue
            seen.add(h)
            rows.append({'texto':text,'etiqueta':label,'fuente':name,'row_index':int(i),'hash':h})
            added+=1
        stats.append({'file':name,'exists':True,'rows':int(len(df)),'added_unique':added,'duplicates':dup,'invalid':invalid})
    out=pd.DataFrame(rows)
    out.to_csv(args.out_csv, index=False, encoding='utf-8')
    manifest={'generated':'phase3-provenance','total_unique_rows':int(len(out)),'label_counts':out['etiqueta'].value_counts().to_dict() if not out.empty else {},'sources':stats,
              'warning':'No es el corpus original de 6400 muestras del modelo de producción; es una reconstrucción trazable desde archivos disponibles.'}
    Path(args.out_json).write_text(json.dumps(manifest,indent=2,ensure_ascii=False),encoding='utf-8')
    print(json.dumps({'out_csv':args.out_csv,'total_unique_rows':len(out),'labels':manifest['label_counts']},ensure_ascii=False))

if __name__=='__main__':
    main()
