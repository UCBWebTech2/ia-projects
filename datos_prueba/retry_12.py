import base64, json, os, time, uuid, subprocess
from pathlib import Path
import requests
from cryptography.hazmat.primitives import hashes, serialization
from cryptography.hazmat.primitives.asymmetric import padding

ROOT = Path(r"C:\ia projects")
private_key = serialization.load_pem_private_key((ROOT / "secrets/internal-jwt-private-dev.pem").read_bytes(), password=None)

def b64(v): return base64.urlsafe_b64encode(v).rstrip(b"=").decode()
def make_token():
    now = int(time.time())
    h = b64(json.dumps({"alg":"RS256","typ":"JWT"}, separators=(",",":")).encode())
    p = b64(json.dumps({"sub":"estudio-juridico-api","iss":"estudio-juridico-api","aud":"ocr-api","permissions":["process"],"actor_user_id":"1","operation":"SubirArchivo","jti":str(uuid.uuid4()),"iat":now,"exp":now+60}, separators=(",",":")).encode())
    signing = f"{h}.{p}".encode()
    sig = private_key.sign(signing, padding.PKCS1v15(), hashes.SHA256())
    return f"{h}.{p}.{b64(sig)}"

def esc(s): return str(s or "").replace("'", "''")

for doc_id, path in [(1, r"C:\ia projects\datos_prueba\judiciales\CIVIL_01_Demanda_Cumplimiento_Contrato.docx"), (2, r"C:\ia projects\datos_prueba\judiciales\CIVIL_02_Contestacion_Demanda.docx")]:
    with open(path, "rb") as f:
        r = requests.post("http://localhost:8000/SubirArchivo", headers={"Authorization": f"Bearer {make_token()}"}, files={"file": (os.path.basename(path), f, "application/vnd.openxmlformats-officedocument.wordprocessingml.document")}, timeout=180)
    print(doc_id, r.status_code)
    if r.status_code != 200:
        print(r.text[:300])
        continue
    data = r.json()
    text = data.get("extracted_text", "")
    entities = data.get("entities", [])
    sql = ["SET NOCOUNT ON;", f"UPDATE dbo.Documentos SET contenidoDocumento=N'{esc(text)}', fechaModificacion=GETDATE() WHERE idDoc={doc_id};", f"DELETE FROM dbo.Indices WHERE idDoc={doc_id};"]
    for e in entities:
        t = esc(e.get("termino", "")); tp = esc(e.get("tipoTermino", "")); fr = int(e.get("frecuencia", 1) or 1)
        if t: sql.append(f"INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES ({doc_id},N'{t}',N'{tp}',{fr});")
    sp = ROOT / f"datos_prueba/tmp_update_doc_{doc_id}.sql"
    sp.write_text("\n".join(sql), encoding="utf-8")
    subprocess.run(["docker","cp",str(sp),f"ejz-db:/tmp/tmp_update_doc_{doc_id}.sql"], check=True)
    res = subprocess.run(["docker","exec","ejz-db","/opt/mssql-tools18/bin/sqlcmd","-S","localhost","-U","sa","-P","YourStrong!Passw0rd","-C","-d","BDProyecto","-i",f"/tmp/tmp_update_doc_{doc_id}.sql"], capture_output=True, text=True)
    print("db", res.returncode, "entities", len(entities))
