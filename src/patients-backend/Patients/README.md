# Patients Microservice

Ce microservice fait partie de l'architecture de l'application de dépistage du diabète de type 2.

## Architecture

Le microservice Patients est structuré selon une architecture en couches:

- **Domain**: Contient les entités métier, interfaces et services
- **Infrastructure**: Implémentation des repositories pour l'accès aux données
- **API**: Controllers REST pour exposer les fonctionnalités

## Base de données

La base de données est normalisée (3NF) pour garantir la qualité des données:
- Table Patient avec les informations personnelles de base
- Table Address pour stocker les adresses de manière normalisée

## Fonctionnalités implémentées (Sprint 1)

- Vue des informations personnelles des patients
- Mise à jour des informations personnelles
- Ajout d'informations personnelles des patients
- Authentification sécurisée avec Identity et JWT

## Configuration requise

- .NET 9
- SQL Server

## Utilisation

### Exécution locale

1. Assurez-vous que la chaîne de connexion dans `appsettings.json` pointe vers votre instance SQL Server
2. Exécutez les migrations: `dotnet ef database update`
3. Démarrez l'application: `dotnet run`

### Utilisation de Docker

```bash
docker build -t patients-microservice .
docker run -p 8080:80 patients-microservice
```

## Authentification

L'API est sécurisée avec JWT. Pour obtenir un token:

```
POST /api/auth/login
{
  "username": "admin",
  "password": "Admin123!"
}
```

Utilisez le token retourné dans les requêtes subséquentes avec un header:
`Authorization: Bearer {token}`

## Green Code

Pour respecter les principes du Green Code, ce microservice:

- Utilise des requêtes optimisées avec Entity Framework (chargement explicite des relations)
- Minimise les transferts de données avec des DTOs spécifiques
- Est conteneurisé pour une utilisation efficace des ressources