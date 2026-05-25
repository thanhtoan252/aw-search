#!/bin/bash
set -e

BAK_FILE=/var/opt/mssql/AdventureWorks2022.bak
BAK_URL="https://github.com/Microsoft/sql-server-samples/releases/download/adventureworks/AdventureWorks2022.bak"
MIN_BAK_BYTES=200000000

download_backup() {
    local tmp_file="${BAK_FILE}.download"
    local bytes

    echo "[init] Downloading AdventureWorks2022.bak..."
    rm -f "$tmp_file"
    wget -qO "$tmp_file" "$BAK_URL"
    bytes=$(stat -c%s "$tmp_file")
    if [ "$bytes" -lt "$MIN_BAK_BYTES" ]; then
        echo "[init] Downloaded backup is incomplete (${bytes} bytes)."
        rm -f "$tmp_file"
        exit 1
    fi
    mv "$tmp_file" "$BAK_FILE"
}

# Start SQL Server in the background
/opt/mssql/bin/sqlservr &
SQLPID=$!

echo "[init] Waiting for SQL Server to be ready..."
for i in $(seq 1 60); do
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -b -Q "SELECT 1" -No > /dev/null 2>&1 && break
    sleep 2
done

# Check whether the database already exists (volume reuse across restarts)
DB_EXISTS=$(/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" \
    -b -Q "SET NOCOUNT ON; SELECT name FROM sys.databases WHERE name='AdventureWorks2022'" \
    -No -h -1 2>/dev/null | tr -d '[:space:]')

if [ -z "$DB_EXISTS" ]; then
    if [ ! -f "$BAK_FILE" ]; then
        download_backup
    fi

    BAK_BYTES=$(stat -c%s "$BAK_FILE")
    if [ "$BAK_BYTES" -lt "$MIN_BAK_BYTES" ]; then
        echo "[init] Existing backup is incomplete (${BAK_BYTES} bytes), re-downloading..."
        rm -f "$BAK_FILE"
        download_backup
    fi

    if ! /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -b -No -Q "RESTORE HEADERONLY FROM DISK = '$BAK_FILE'" > /dev/null; then
        echo "[init] Existing backup is invalid, re-downloading..."
        rm -f "$BAK_FILE"
        download_backup
    fi

    echo "[init] Restoring AdventureWorks2022..."
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -b -No -Q "
        RESTORE DATABASE AdventureWorks2022
        FROM DISK = '$BAK_FILE'
        WITH
            MOVE 'AdventureWorks2022' TO '/var/opt/mssql/data/AdventureWorks2022.mdf',
            MOVE 'AdventureWorks2022_log' TO '/var/opt/mssql/data/AdventureWorks2022_log.ldf',
            NOUNLOAD, STATS = 10"
    echo "[init] Database restored."
else
    echo "[init] AdventureWorks2022 already exists, skipping restore."
fi

wait $SQLPID
