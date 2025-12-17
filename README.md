# Application de Dépistage du Diabète de Type 2

## 📋 Présentation

Ce projet a été développé dans le cadre de la formation **Développeur Backend .NET** d'OpenClassrooms. Il s'agit d'une application distribuée permettant de gérer les informations des patients et d'évaluer leur risque de développer un diabète de type 2.

### Contexte du projet

L'application répond à la demande d'une clinique de santé qui souhaite automatiser le dépistage du diabète de type 2 chez ses patients. Le système analyse les données personnelles des patients (âge, genre) ainsi que leurs notes médicales pour calculer un niveau de risque personnalisé.

## 🏗️ Architecture

L'application est construite selon une **architecture microservices** avec les composants suivants :

```
┌─────────────────┐
│  Frontend       │  (Blazor Server)
│  Patients-      │  Port: 5000
│  Frontend       │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  API Gateway    │  (Ocelot)
│  Authentication │  Port: 7000 
│  + JWT          │
└────────┬────────┘
         │
         ├──────────────┬──────────────┬──────────────┐
         ▼              ▼              ▼              ▼
┌──────────────┐ ┌─────────────┐ ┌──────────────┐ ┌─────────────┐
│  Patients    │ │   Notes     │ │ Assessments  │ │  SQL Server │
│  Backend     │ │  Backend    │ │  Backend     │ │  (AuthDB)   │
│  Port: 5082  │ │ Port: 5083  │ │ Port: 5084   │ │             │
└──────────────┘ └─────────────┘ └──────────────┘ └─────────────┘
     │                  │
     ▼                  ▼
┌──────────┐      ┌──────────┐
│SQL Server│      │ MongoDB  │
│(PatientsDb)     │(NotesDb) │
└──────────┘      └──────────┘
```

### 🔐 Sécurité

- **Authentification** : ASP.NET Core Identity avec tokens JWT
- **Autorisation** : Tous les endpoints backend sont protégés par JWT
- **Gateway** : Point d'entrée unique gérant l'authentification et la distribution des requêtes
- **Propagation du token** : Le JWT est propagé automatiquement entre microservices

## 🎯 Microservices

### 1. API Gateway (Ocelot)
- **Technologie** : ASP.NET Core 9 + Ocelot
- **Responsabilités** :
  - Point d'entrée unique de l'application
  - Routage des requêtes vers les microservices downstream
  - Gestion de l'authentification (ASP.NET Core Identity)
  - Génération et validation des tokens JWT
  - Seed automatique de l'utilisateur admin au démarrage
- **Base de données** : SQL Server (AuthDb)

### 2. Patients Backend
- **Technologie** : ASP.NET Core 9
- **Responsabilités** :
  - CRUD complet des patients
  - Gestion des informations personnelles (nom, prénom, date de naissance, genre)
  - Gestion des adresses
  - Audit des modifications (CreatedBy, UpdatedBy, timestamps)
  - Soft delete des entités
- **Base de données** : SQL Server (PatientsDb) - Normalisée 3NF
- **Normalisation 3NF** :
  - Table `Patients` : informations personnelles
  - Table `Addresses` : adresses séparées pour éviter la redondance
  - Relation 1-N entre Address et Patient

### 3. Notes Backend
- **Technologie** : ASP.NET Core 9
- **Responsabilités** :
  - Gestion des notes médicales des patients
  - CRUD des notes
  - Recherche des notes par patient
  - Seed automatique de notes de démonstration
- **Base de données** : MongoDB (NotesDb)

### 4. Assessments Backend
- **Technologie** : ASP.NET Core 9
- **Responsabilités** :
  - Calcul du niveau de risque diabétique
  - Analyse basée sur l'âge, le genre et les termes déclencheurs dans les notes
  - Communication avec Patients Backend et Notes Backend via l'API Gateway
  - Propagation automatique du token JWT
- **Niveaux de risque** :
  - `None` : Aucun risque
  - `BorderLine` : Limite (>30 ans avec 2+ déclencheurs, ou 6+ déclencheurs)
  - `InDanger` : En danger (>30 ans avec 6+ déclencheurs, ou hommes <30 ans avec 3+ déclencheurs, ou femmes <30 ans avec 4+ déclencheurs)
  - `EarlyOnset` : Apparition précoce (hommes <30 ans avec 5+ déclencheurs, ou femmes <30 ans avec 7+ déclencheurs)

### 5. Patients Frontend
- **Technologie** : Blazor Server (.NET 9)
- **Responsabilités** :
  - Interface utilisateur pour la gestion des patients
  - Authentification des utilisateurs
  - Affichage et modification des informations patients
  - Gestion des notes médicales
  - Visualisation du niveau de risque diabétique
- **Communication** : Appels HTTP vers l'API Gateway

## 🚀 Installation et Démarrage

### Prérequis
- Docker et Docker Compose
- .NET 9 SDK (pour le développement local)

### Lancement avec Docker

1. **Cloner le repository**
```bash
git clone https://github.com/Gregb342/OC-Rendus-P10
cd OC-Rendus-P10
```

2. **Démarrer tous les services**
```bash
cd src
docker-compose up -d
```

3. **Vérifier que tous les services sont démarrés**
```bash
docker-compose ps
```

Les services seront accessibles aux adresses suivantes :
- **Frontend** : http://localhost:5000
- **API Gateway** : http://localhost:7000
- **Patients Backend** : http://localhost:5082
- **Notes Backend** : http://localhost:5083
- **Assessments Backend** : http://localhost:5084
- **SQL Server** : localhost:1433
- **MongoDB** : localhost:27017

### 🔑 Connexion Administrateur

Un utilisateur admin est créé automatiquement au démarrage de l'API Gateway :

```
Username: admin
Password: Admin123!
```

Pour obtenir un token JWT, effectuez une requête POST vers :
```
POST http://localhost:7000/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin123!"
}
```

La réponse contiendra un token JWT à utiliser dans l'en-tête `Authorization: Bearer <token>` pour les requêtes suivantes.

### 🧪 Tests

Chaque microservice dispose de tests unitaires :

```bash
# Tests Patients Backend
cd src/patients-backend/PatientsTests
dotnet test

# Tests Assessments Backend
cd src/Assessments_backend/Assessments_backend.Tests
dotnet test

# Tests Notes Backend
cd src/notes-backend/unitTests/Notes.UnitTests
dotnet test
```

## 📊 Base de Données

### SQL Server (Patients)
La base de données est normalisée en **3ème Forme Normale (3NF)** pour garantir :
- Élimination des redondances
- Intégrité des données
- Conformité aux normes ISO du client

**Structure** :
- Table `Patients` (Id, FirstName, LastName, DateOfBirth, Gender, PhoneNumber, AddressId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted)
- Table `Addresses` (Id, Street, City, PostalCode, Country, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted)

### MongoDB (Notes)
Collection `Notes` contenant :
- PatientId
- Content (contenu de la note)
- CreatedAt
- UpdatedAt

## 🌱 Green Code - Suggestions d'Actions

Dans le cadre de la politique environnementale du client, voici des suggestions pour appliquer les principes du Green Code à ce projet :

### 1. Optimisation des requêtes
- ✅ Utiliser la pagination pour limiter le volume de données transférées
- ✅ Implémenter des projections (Select) pour ne récupérer que les champs nécessaires
- ⚠️ Mettre en place du caching Redis pour réduire les appels aux bases de données

### 2. Optimisation des images Docker
- ✅ Utilisation d'images multi-stage pour réduire la taille des images
- ⚠️ Analyser et réduire la taille des images de production (actuellement ~210MB par service)
- ⚠️ Utiliser des images Alpine Linux pour réduire l'empreinte

### 3. Optimisation du code
- ✅ Utilisation de async/await pour optimiser l'utilisation des threads
- ✅ Injection de dépendances avec scopes appropriés
- ⚠️ Implémenter des health checks pour éviter les restart inutiles
- ⚠️ Ajouter des métriques de performance (Application Insights, Prometheus)

### 4. Infrastructure
- ⚠️ Mettre en place l'auto-scaling basé sur la charge réelle
- ⚠️ Utiliser des bases de données serverless pour les environnements de développement
- ⚠️ Implémenter une stratégie de mise en veille des services non utilisés

### 5. Logging et Monitoring
- ✅ Logging structuré avec Serilog vers Graylog
- ⚠️ Configurer des niveaux de log adaptatifs (Verbose en dev, Warning/Error en prod)
- ⚠️ Archiver et compresser les anciens logs

### 6. Bonnes pratiques
- ⚠️ Optimiser les requêtes N+1 avec Include/Join appropriés
- ⚠️ Implémenter des stratégies de retry avec backoff exponentiel
- ⚠️ Utiliser des Connection Pooling optimisés

## 🛠️ Technologies Utilisées

- **Backend** : ASP.NET Core 9, C# 13
- **Frontend** : Blazor Server
- **API Gateway** : Ocelot
- **Authentification** : ASP.NET Core Identity + JWT
- **Bases de données** : SQL Server, MongoDB
- **ORM** : Entity Framework Core
- **Logging** : Serilog + Graylog
- **Tests** : xUnit, Moq
- **Containerisation** : Docker, Docker Compose

## 📝 Configuration Requise pour le Développement Local

### appsettings.json communs

Tous les microservices partagent les mêmes paramètres JWT :
```json
{
  "JWT": {
    "ValidIssuer": "https://localhost:5001",
    "ValidAudience": "https://localhost:5001",
    "Secret": "SuperSecretKeyForDevThatIsAtLeast32CharactersLong123456789"
  }
}
```

### Chaînes de connexion

**SQL Server (Patients & API Gateway)** :
```
Server=localhost;Database=PatientsDb;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=True;
```

**MongoDB (Notes)** :
```
mongodb://admin:AdminPassword123@localhost:27017
```

## 📚 Documentation API

Swagger UI est disponible en mode développement sur chaque backend :
- Patients : http://localhost:5082/swagger
- Notes : http://localhost:5083/swagger
- Assessments : http://localhost:5084/openapi

## 👥 Contributeurs

Projet académique - Formation OpenClassrooms Développeur Backend .NET

## 📄 Licence

Ce projet est à usage éducatif dans le cadre de la formation OpenClassrooms.
