import pyodbc

connection = None
try:
    # Configuración de la conexión utilizando la cadena proporcionada en C#
    connection = pyodbc.connect(
        'DRIVER={SQL Server};'
        'SERVER=DESKTOP-J5GE4F8;'
        'DATABASE=BDPruebaProyecto;'
        'Trusted_Connection=yes;'
        'TrustServerCertificate=yes;'
    )
    print("Conexión exitosa.")

    # Creación de un cursor y ejecución de una consulta de prueba
    cursor = connection.cursor()
    cursor.execute("SELECT @@version;")
    row = cursor.fetchone()
    if row is not None:
        print("Versión del servidor de SQL Server:", row[0])
    else:
        print("No se obtuvo información de la versión del servidor.")
    # Ejemplo de consulta adicional
    cursor.execute("SELECT * FROM Usuarios")
    rows = cursor.fetchall()
    for row in rows:
        print(row)

except Exception as ex:
    print("Error durante la conexión:", ex)

finally:
    # Cerrar la conexión
    if connection is not None:
        connection.close()
    print("La conexión ha finalizado.")
