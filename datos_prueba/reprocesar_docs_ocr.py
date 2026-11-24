import base64, json, os, time, uuid, subprocess, sys
from pathlib import Path
import requests
from cryptography.hazmat.primitives import hashes, serialization
from cryptography.hazmat.primitives.asymmetric import padding

ROOT = Path(r"C:\ia projects")
key_path = ROOT / "secrets" / "internal-jwt-private-dev.pem"
private_key = serialization.load_pem_private_key(key_path.read_bytes(), password=None)

def b64(v: bytes) -> str:
    return base64.urlsafe_b64encode(v).rstrip(b"=").decode()

def make_token(op="SubirArchivo", perms=None):
    now = int(time.time())
    header = {"alg":"RS256","typ":"JWT"}
    payload = {
        "sub":"estudio-juridico-api","iss":"estudio-juridico-api","aud":"ocr-api",
        "permissions": perms or ["process"], "actor_user_id":"1","operation":op,
        "jti": str(uuid.uuid4()), "iat": now, "exp": now + 60
    }
    h = b64(json.dumps(header,separators=(",",":")).encode())
    p = b64(json.dumps(payload,separators=(",",":")).encode())
    signing = f"{h}.{p}".encode()
    sig = private_key.sign(signing, padding.PKCS1v15(), hashes.SHA256())
    return f"{h}.{p}.{b64(sig)}"

docs = [
    (1, r"C:\ia projects\datos_prueba\judiciales\CIVIL_01_Demanda_Cumplimiento_Contrato.docx"),
    (2, r"C:\ia projects\datos_prueba\judiciales\CIVIL_02_Contestacion_Demanda.docx"),
    (3, r"C:\ia projects\datos_prueba\judiciales\CIVIL_03_Sentencia_Cumplimiento.docx"),
    (4, r"C:\ia projects\datos_prueba\judiciales\PENAL_01_Denuncia_Estafa.docx"),
    (5, r"C:\ia projects\datos_prueba\judiciales\PENAL_02_Querella_Lesiones_Graves.docx"),
    (6, r"C:\ia projects\datos_prueba\judiciales\PENAL_03_Sentencia_Estafa.docx"),
]

def esc(s):
    if s is None: return ""
    return str(s).replace("'", "''")

for doc_id, path in docs:
    token = make_token()
    with open(path, "rb") as f:
        r = requests.post("http://localhost:8000/SubirArchivo", headers={"Authorization": f"Bearer {token}"}, files={"file": (os.path.basename(path), f, "application/vnd.openxmlformats-officedocument.wordprocessingml.document")}, timeout=180)
    print(doc_id, r.status_code)
    if r.status_code != 200:
        print(r.text[:500])
        continue
    data = r.json()
    text = data.get("extracted_text","")
    entities = data.get("entities", [])
    # build SQL
    sql_lines = ["SET NOCOUNT ON;"]
    sql_lines.append(f"UPDATE dbo.Documentos SET contenidoDocumento=N'{esc(text)}', fechaModificacion=GETDATE() WHERE idDoc={doc_id};")
    sql_lines.append(f"DELETE FROM dbo.Indices WHERE idDoc={doc_id};")
    for e in entities:
        termino = esc(e.get("termino",""))
        tipo = esc(e.get("tipoTermino",""))
        freq = int(e.get("frecuencia",1) or 1)
        if termino:
            sql_lines.append(f"INSERT INTO dbo.Indices (idDoc,termino,tipoTermino,frecuencia) VALUES ({doc_id},N'{termino}',N'{tipo}',{freq});")
    sql_path = ROOT / f"datos_prueba/tmp_update_doc_{doc_id}.sql"
    sql_path.write_text("\n".join(sql_lines), encoding="utf-8")
    subprocess.run(["docker","cp",str(sql_path),f"ejz-db:/tmp/tmp_update_doc_{doc_id}.sql"], check=True)
    res = subprocess.run(["docker","exec","ejz-db","/opt/mssql-tools18/bin/sqlcmd","-S","localhost","-U","sa","-P","YourStrong!Passw0rd","-C","-d","BDProyecto","-i",f"/tmp/tmp_update_doc_{doc_id}.sql"], capture_output=True, text=True)
    print("db", res.returncode, res.stdout[-200:] if res.stdout else "", res.stderr[-200:] if res.stderr else "")
    print("entities", len(entities))
