# Datos de prueba y entrenamiento

Guía para cargar las semillas SQL de esta carpeta y entrenar el clasificador
Civil/Penal con Docker Compose. Ejecuta los comandos de PowerShell desde la raíz
del repositorio.

## Requisitos

- Docker Desktop en ejecución y los servicios del proyecto iniciados.
- Una base de datos de desarrollo llamada `BDProyecto`.
- Los archivos de muestra de `judiciales/`.

Inicia el entorno si todavía no está activo:

```powershell
docker compose up -d --build
```

## Cargar las semillas

Las semillas SQL se ejecutan en este orden: primero crean categorías y procesos;
después insertan los seis documentos.

```powershell
docker cp .\datos_prueba\seed_datos_prueba.sql ejz-db:/tmp/seed_datos_prueba.sql
docker exec ejz-db bash -lc '/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -d "${DB_NAME:-BDProyecto}" -i /tmp/seed_datos_prueba.sql -b'

docker cp .\datos_prueba\seed_documentos_prueba.sql ejz-db:/tmp/seed_documentos_prueba.sql
docker exec ejz-db bash -lc '/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -d "${DB_NAME:-BDProyecto}" -i /tmp/seed_documentos_prueba.sql -b'
```

Ambos scripts son idempotentes por nombre. `seed_documentos_prueba.sql` asigna
los IDs 1–3 de procesos y 1–2 de categorías; úsalo únicamente en una base de
prueba donde esas referencias correspondan a los datos que inserta
`seed_datos_prueba.sql`. La base también debe tener el usuario con ID 1.

Verifica la cantidad de documentos:

```powershell
docker exec ejz-db bash -lc '/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -d "${DB_NAME:-BDProyecto}" -Q "SELECT COUNT(*) AS documentos FROM dbo.Documentos" -W'
```

`seed_usuarios_procesos.sql` es opcional: agrega cuentas de prueba y actualiza
los nombres de los procesos con IDs 1–3. Úsalo solo en una base de desarrollo.

## NER de los documentos

Insertar los documentos con SQL **no ejecuta OCR ni NER**. Para procesar un
documento nuevo, usa la carga habitual de documentos de la aplicación
(`POST /api/Documento/Subir`): la API llama al servicio OCR y guarda el texto y
las entidades extraídas. Evita volver a subir los seis documentos después de
sembrarlos, porque esa ruta crea documentos nuevos. Para reprocesar documentos
ya sembrados, hace falta actualizar sus registros existentes; los scripts SQL
de semillas no realizan ese reproceso.

La verificación de entidades ya persistidas puede hacerse con:

```sql
SELECT d.nombreDocumento,
       COUNT(DISTINCT i.idIndice) AS indicesNER,
       COUNT(DISTINCT e.idEntidad) AS entidadesNER
FROM dbo.Documentos d
LEFT JOIN dbo.Indices i ON i.idDoc = d.idDoc
LEFT JOIN dbo.DocumentoEntidades e ON e.idDoc = d.idDoc
WHERE d.nombreDocumento LIKE 'CIVIL[_]%'
   OR d.nombreDocumento LIKE 'PENAL[_]%'
GROUP BY d.nombreDocumento
ORDER BY d.nombreDocumento;
```

## Entrenar Naive Bayes

El entrenamiento usa `OCR_API/files/train_4000.csv` (3.200 filas) y evalúa con
`OCR_API/files/test_4000.csv` (800 filas). El test está congelado: no lo edites
ni lo uses para agregar ejemplos de entrenamiento.

```powershell
docker compose exec ocr-api python OCR_API/scripts/entrenar_v2.py
docker compose restart ocr-api
curl.exe -fsS http://localhost:8000/health
```

El modelo y su metadata se guardan en el volumen Docker `ocr_models` como
`modelo_civil_penal_v1.joblib` y `modelo_civil_penal_v1.metadata.json`. El
reinicio hace que la API cargue el artefacto actualizado. Revisa los logs si
quieres confirmar la carga:

```powershell
docker compose logs --tail 40 ocr-api
```

El script aplica un control de aceptación frente al macro-F1 registrado
anteriormente. Si el resultado empeora, puede rechazar guardar el candidato;
no uses `--force` salvo que hayas revisado y aceptado esa regresión.

En la última ejecución con estos archivos, el resultado fue accuracy y macro-F1
de `0.9637` sobre el conjunto congelado de prueba.

## Semillas relacionadas

- `seed_datos_prueba.sql`: categorías y procesos.
- `seed_documentos_prueba.sql`: documentos de muestra.
- `seed_usuarios_procesos.sql`: usuarios y nombres alternativos de procesos.
- `judiciales/`: archivos DOCX originales.
