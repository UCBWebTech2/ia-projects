import json, subprocess
from pathlib import Path
from docx import Document
ROOT=Path(r'C:\ia projects')
doc=Document(r'C:\ia projects\datos_prueba\judiciales\CIVIL_01_Demanda_Cumplimiento_Contrato.docx')
text='\n'.join(p.text for p in doc.paragraphs if p.text.strip())
tmp=ROOT/'datos_prueba/tmp_doc_1.txt'; tmp.write_text(text,encoding='utf-8')
subprocess.run(['docker','cp',str(tmp),'ejz-ocr-api:/tmp/doc.txt'],check=True)
py=r"""from OCR_API.scripts.EntityRuler import find_entities, eliminar_duplicados_semanticos; import json; text=open('/tmp/doc.txt',encoding='utf-8').read(); ents=find_entities(text, eliminar_repetidos=False, incluir_genericas=True); ents=eliminar_duplicados_semanticos(ents, incluir_frecuencia=True); print(json.dumps(ents,ensure_ascii=False))"""
res=subprocess.run(['docker','exec','ejz-ocr-api','python','-c',py],capture_output=True,text=True)
print(res.stderr[-200:] if res.stderr else '')
ents=json.loads(res.stdout.strip().splitlines()[-1])
for e in ents: print(e['label'],'|',e['text'])