import json, os, subprocess
from pathlib import Path
from docx import Document

ROOT = Path(r"C:\ia projects")
docs = [
    (1, r"C:\ia projects\datos_prueba\judiciales\CIVIL_01_Demanda_Cumplimiento_Contrato.docx"),
    (2, r"C:\ia projects\datos_prueba\judiciales\CIVIL_02_Contestacion_Demanda.docx"),
    (3, r"C:\ia projects\datos_prueba\judiciales\CIVIL_03_Sentencia_Cumplimiento.docx"),
    (4, r"C:\ia projects\datos_prueba\judiciales\PENAL_01_Denuncia_Estafa.docx"),
    (5, r"C:\ia projects\datos_prueba\judiciales\PENAL_02_Querella_Lesiones_Graves.docx"),
    (6, r"C:\ia projects\datos_prueba\judiciales\PENAL_03_Sentencia_Estafa.docx"),
]

def esc(s): return str(s or "").replace("'", "''")

for doc_id, path in docs:
    doc = Document(path)
    text = "\n".join(p.text for p in doc.paragraphs if p.text.strip())
    # get entities from container
    tmp_txt = ROOT / "datos_prueba" / f"tmp_doc_{doc_id}.txt"
    tmp_txt.write_text(text, encoding="utf-8")
    subprocess.run(["docker","cp",str(tmp_txt),"ejz-ocr-api:/tmp/doc.txt"], check=True)
    py = (
        "from OCR_API.scripts.EntityRuler import find_entities, eliminar_duplicados_semanticos;"
        "import json;"
        "text=open('/tmp/doc.txt',encoding='utf-8').read();"
        "ents=find_entities(text, eliminar_repetidos=False, incluir_genericas=True);"
        "ents=eliminar_duplicados_semanticos(ents, incluir_frecuencia=True);"
        "print(json.dumps(ents,ensure_ascii=False))"
    )
    res = subprocess.run(["docker","exec","ejz-ocr-api","python","-c",py], capture_output=True, text=True, encoding="utf-8", errors="replace")
    if res.returncode != 0:
        print(doc_id, "ner error", (res.stderr or "")[-300:])
        entities = []
    else:
        out = (res.stdout or "").strip().splitlines()[-1]
        entities = json.loads(out)
    # build sql safely
    sql = ["SET NOCOUNT ON;"]
    sql.append(f"UPDATE dbo.Documentos SET contenidoDocumento=N'{esc(text)}', fechaModificacion=GETDATE() WHERE idDoc={doc_id};")
    sql.append(f"DELETE FROM dbo.Indices WHERE idDoc={doc_id};")
    for e in entities:
        termino = esc(e.get("text",""))
        tipo = esc(e.get("label",""))
        freq = int(e.get("frequency",1) or 1)
        if termino:
            sql.append(f"INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES ({doc_id},N'{termino}',N'{tipo}',{freq});")
    sp = ROOT / "datos_prueba" / f"tmp_update_doc_{doc_id}.sql"
    sp.write_text("\n".join(sql), encoding="utf-8")
    subprocess.run(["docker","cp",str(sp),f"ejz-db:/tmp/tmp_update_doc_{doc_id}.sql"], check=True)
    r2 = subprocess.run(["docker","exec","ejz-db","/opt/mssql-tools18/bin/sqlcmd","-S","localhost","-U","sa","-P","YourStrong!Passw0rd","-C","-d","BDProyecto","-i",f"/tmp/tmp_update_doc_{doc_id}.sql"], capture_output=True, text=True)
    print(doc_id, "len", len(text), "entities", len(entities), "db", r2.returncode)
