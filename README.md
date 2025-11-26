# OC-Rendus-P10

Projet 10 OpenClassrooms : Développez une solution en microservices pour votre client

## 📋 Description

Solution complète de gestion de patients développée avec une architecture microservices. Le projet propose une interface web pour gérer les informations des patients avec un système d'authentification sécurisé, un backend RESTful, et une centralisation des logs via Graylog.

## 🏗️ Architecture

La solution est composée de plusieurs services dockerisés :

- **Frontend** : Application web Blazor Server (.NET) pour l'interface utilisateur
- **API Gateway** : Passerelle Ocelot pour router les requêtes vers les microservices
- **Patients Backend** : API REST (.NET) gérant les opérations CRUD sur les patients
- **Graylog** : Stack de centralisation des logs (Graylog + MongoDB + OpenSearch)
- **PostgreSQL** : Base de données relationnelle pour les patients

### Stack technique

- **.NET 9.0** (ASP.NET Core, Blazor Server)
- **Ocelot** (API Gateway)
- **PostgreSQL** (Base de données)
- **Entity Framework Core** (ORM)
- **Graylog** (Centralisation des logs)
- **Docker & Docker Compose** (Conteneurisation)

## 📦 Prérequis

- Docker Desktop installé et en cours d'exécution
- Docker Compose
- Port 80, 5000, 1433, 9000, 1514, 12201, 27017 disponibles

## 🚀 Installation et lancement

### 1. Cloner le repository

```bash
git clone [URL_DU_REPO]
cd OC-Rendus-P10
```

### 2. Lancer l'ensemble de la solution

Depuis la racine du projet :

```bash
cd src
docker-compose up -d
```

Cette commande va :
- Créer et démarrer tous les conteneurs
- Initialiser les bases de données PostgreSQL
- Configurer le réseau entre les services

### 3. Vérifier que les services sont actifs

```bash
docker-compose ps
```

Tous les services doivent être à l'état "Up".

## 🌐 Accès aux services

Une fois les conteneurs lancés :

- **Frontend (Application web)** : http://localhost:5000
- **API Gateway** : http://localhost:7000
- **Patients API** : http://localhost:5082 (via le gateway)
- **Graylog (Logs)** : http://localhost:9000
  - Login par défaut : `admin` / `admin`

## 📝 Utilisation

### Authentification

L'application nécessite une authentification pour accéder aux fonctionnalités :
- Utilisez les endpoints d'authentification via `/api/auth/login`

### Gestion des patients

Via l'interface web, vous pouvez :
- **Créer** un nouveau patient avec ses informations personnelles et son adresse
- **Consulter** la liste des patients
- **Modifier** les informations d'un patient existant
- **Supprimer** un patient (soft delete)

### API REST

Les endpoints suivants sont disponibles via l'API Gateway :

```
GET    /api/patients          - Liste tous les patients
GET    /api/patients/{id}     - Récupère un patient par son ID
POST   /api/patients          - Crée un nouveau patient
PUT    /api/patients/{id}     - Met à jour un patient
DELETE /api/patients/{id}     - Supprime un patient (soft delete)
POST   /api/auth/login        - Authentification
```

## 🧪 Tests

Des tests unitaires sont disponibles dans le projet `Patients.Tests`.

Pour lancer les tests :

```bash
cd src/patients-backend/Patients
dotnet test
```

## 🗃️ Base de données

### Structure

Le service Patients utilise PostgreSQL avec les entités principales :
- **Patient** : Informations personnelles (nom, prénom, date de naissance, genre, téléphone, email)
- **Address** : Adresse du patient (rue, ville, code postal)

### Migrations

Les migrations Entity Framework Core sont automatiquement appliquées au démarrage du service.

Pour créer une nouvelle migration :

```bash
cd src/patients-backend/Patients
dotnet ef migrations add NomDeLaMigration
```

## 📊 Monitoring et Logs

### Graylog

Tous les logs des microservices sont centralisés dans Graylog :

1. Accédez à http://localhost:9000
2. Connectez-vous (admin/admin)
3. Configurez les inputs GELF UDP sur le port 12201
4. Consultez les logs en temps réel

## 🛑 Arrêter l'application

```bash
cd src
docker-compose down
```

Pour supprimer également les volumes (données) :

```bash
docker-compose down -v
```

## 🔧 Configuration

### API Gateway (Ocelot)

La configuration des routes est définie dans `src/api-gateway/ApiGateway/ApiGateway/ocelot.json`

### Variables d'environnement

Les configurations sont définies dans le `docker-compose.yml` :
- Chaînes de connexion aux bases de données
- Ports d'exposition des services
- Configuration Graylog

## 📁 Structure du projet

```
src/
├── docker-compose.yml           # Orchestration des conteneurs
├── init-databases.sh            # Script d'initialisation BDD
├── api-gateway/                 # API Gateway Ocelot
│   └── ApiGateway/
├── frontend/                    # Frontend Blazor Server
│   └── Patients-Frontend/
├── patients-backend/            # API REST Patients
│   └── Patients/
│       ├── Controllers/         # Contrôleurs API
│       ├── Domain/              # Entités et services métier
│       ├── Infrastructure/      # Repositories et configurations
│       ├── DTOs/                # Objets de transfert de données
│       └── Migrations/          # Migrations EF Core
└── graylog/                     # Configuration Graylog
    └── docker-compose.yml
```

## 🔒 Sécurité

- Authentification JWT pour sécuriser les endpoints
- Soft delete pour préserver l'intégrité des données
- Validation des données côté serveur
- Isolation des services via Docker

## 👤 Auteur

Projet réalisé dans le cadre de la formation OpenClassrooms - Développeur d'application .NET

## 📄 Licence

Ce projet est réalisé à des fins pédagogiques dans le cadre du parcours OpenClassrooms.