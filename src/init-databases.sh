#!/bin/bash
# Script d'initialisation des bases de données SQL Server

# Attendre que SQL Server soit prêt
echo "Attente du démarrage de SQL Server..."
sleep 30s

echo "Création des bases de données..."

# Créer les bases de données
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -C -Q "
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'PatientsDb')
BEGIN
    CREATE DATABASE PatientsDb;
END
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'AuthDb')
BEGIN
    CREATE DATABASE AuthDb;
END
GO
"

echo "Bases de données créées avec succès !"
