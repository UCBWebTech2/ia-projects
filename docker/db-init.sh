#!/bin/bash
set -euo pipefail

SA_PASSWORD="${SA_PASSWORD:-YourStrong!Passw0rd}"
DB_NAME="${DB_NAME:-BDProyecto}"
SQLCMD="/opt/mssql-tools18/bin/sqlcmd"
if [ ! -x "$SQLCMD" ]; then
  SQLCMD="/opt/mssql-tools/bin/sqlcmd"
fi

echo "Waiting for SQL Server..."
for i in {1..60}; do
  if "$SQLCMD" -S db -U sa -P "$SA_PASSWORD" -C -Q "SELECT 1" >/dev/null 2>&1; then
    echo "SQL Server is ready."
    break
  fi
  sleep 2
  if [ "$i" -eq 60 ]; then
    echo "SQL Server did not become ready in time." >&2
    exit 1
  fi
done

echo "Ensuring database [$DB_NAME] exists..."
"$SQLCMD" -S db -U sa -P "$SA_PASSWORD" -C -Q "IF DB_ID(N'${DB_NAME}') IS NULL CREATE DATABASE [${DB_NAME}];"

echo "Applying SQL scripts from /scripts..."
shopt -s nullglob
for f in /scripts/*.sql; do
  echo "Applying $f"
  "$SQLCMD" -S db -U sa -P "$SA_PASSWORD" -C -d "$DB_NAME" -i "$f"
done

echo "Database initialization complete."
