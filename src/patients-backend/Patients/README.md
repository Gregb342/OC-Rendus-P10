# Patients Microservice

Ce microservice fait partie de l'architecture de l'application de d�pistage du diab�te de type 2.

## Architecture

Le microservice Patients est structur� selon une architecture en couches:

- **Domain**: Contient les entit�s m�tier, interfaces et services
- **Infrastructure**: Impl�mentation des repositories pour l'acc�s aux donn�es
- **API**: Controllers REST pour exposer les fonctionnalit�s

## Base de donn�es

La base de donn�es est normalis�e (3NF) pour garantir la qualit� des donn�es:
- Table Patient avec les informations personnelles de base
- Table Address pour stocker les adresses de mani�re normalis�e

## Fonctionnalit�s impl�ment�es (Sprint 1)

- Vue des informations personnelles des patients
- Mise � jour des informations personnelles
- Ajout d'informations personnelles des patients
- Authentification s�curis�e avec Identity et JWT

## Configuration requise

- .NET 9
- SQL Server

## Utilisation

### Ex�cution locale

1. Assurez-vous que la cha�ne de connexion dans `appsettings.json` pointe vers votre instance SQL Server
2. Ex�cutez les migrations: `dotnet ef database update`
3. D�marrez l'application: `dotnet run`

### Utilisation de Docker

```bash
docker build -t patients-microservice .
docker run -p 8080:80 patients-microservice
```

## Authentification

L'API est s�curis�e avec JWT. Pour obtenir un token:

```
POST /api/auth/login
{
  "username": "admin",
  "password": "Admin123!"
}
```

Utilisez le token retourn� dans les requ�tes subs�quentes avec un header:
`Authorization: Bearer {token}`

## Green Code

Pour respecter les principes du Green Code, ce microservice:

- Utilise des requ�tes optimis�es avec Entity Framework (chargement explicite des relations)
- Minimise les transferts de donn�es avec des DTOs sp�cifiques
- Est conteneuris� pour une utilisation efficace des ressources