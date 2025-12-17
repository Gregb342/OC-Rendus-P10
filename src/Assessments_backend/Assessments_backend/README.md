# Assessments Backend - Microservice d'évaluation des risques diabétiques

## ?? Description

Microservice .NET 9 pour évaluer le niveau de risque diabétique des patients en fonction de leur âge, genre et notes médicales.

## ??? Architecture

Ce service s'intègre dans une architecture microservices et communique avec :
- **API Gateway (Ocelot)** : Point d'entrée unique avec authentification JWT
- **Patients Backend** : Récupération des données des patients
- **Notes Backend** : Récupération des notes médicales (à implémenter)

### Flux d'authentification

```
Frontend ? API Gateway (JWT) ? Assessments Backend
                                     ?
                            Patients Backend (via API Gateway + JWT)
                                     ?
                            Notes Backend (via API Gateway + JWT)
```

## ?? Fonctionnalités

- ? Évaluation du risque diabétique (None, Borderline, In Danger, Early Onset)
- ? Propagation automatique du token JWT
- ? Logging structuré
- ? Gestion d'erreurs robuste
- ? Tests unitaires

## ?? Configuration

### Variables d'environnement requises

```json
{
  "ApiGateway": {
    "BaseUrl": "http://api-gateway:7000"
  },
  "JWT": {
    "ValidAudience": "http://localhost:7000",
    "ValidIssuer": "http://localhost:7000",
    "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!"
  }
}
```

?? **Important** : Les paramètres JWT doivent être **identiques** dans tous les services (API Gateway, Patients, Assessments).

## ?? API Endpoints

### GET /api/assessment/{id}

Récupère l'évaluation de risque pour un patient.

**Paramètres :**
- `id` : ID du patient (integer)

**Headers requis :**
```
Authorization: Bearer <jwt_token>
```

**Réponse (200 OK) :**
```json
{
  "patientId": 1,
  "riskLevel": "InDanger",
  "assessment": "Le patient présente un risque élevé de diabète"
}
```

**Codes d'erreur :**
- `400` : ID invalide
- `401` : Token manquant ou invalide
- `404` : Patient non trouvé
- `500` : Erreur serveur

## ?? Tests

```bash
cd Assessments_backend.Tests
dotnet test
```

## ?? Docker

### Build de l'image

```bash
docker build -t assessments-backend .
```

### Exécution locale

```bash
docker run -p 5083:80 \
  -e ApiGateway__BaseUrl=http://host.docker.internal:7000 \
  -e JWT__ValidAudience=http://localhost:7000 \
  -e JWT__ValidIssuer=http://localhost:7000 \
  -e JWT__Secret=YourSuperSecretKeyThatIsAtLeast32CharactersLong! \
  assessments-backend
```

## ?? Dépendances

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.3.1" />
```

## ?? Sécurité

- Authentification JWT obligatoire sur tous les endpoints
- Propagation sécurisée du token entre microservices
- Validation de l'expiration du token (3h par défaut)
- HTTPS recommandé en production

## ?? Algorithme d'évaluation des risques

### Critères de déclenchement

L'algorithme analyse les notes médicales du patient pour détecter les termes suivants :
- Hémoglobine A1C, Microalbumine, Taille, Poids, Fumeur, Fumeuse
- Anormal, Cholestérol, Vertiges, Rechute, Réaction, Anticorps

### Niveaux de risque

| Niveau | Conditions |
|--------|-----------|
| **None** | Moins de 2 termes détectés |
| **Borderline** | ?30 ans avec 2-5 termes |
| **In Danger** | ?30 ans avec 6-7 termes OU <30 ans avec 3-4 termes |
| **Early Onset** | ?30 ans avec ?8 termes OU <30 ans avec ?5 termes |

**Bonus Genre** : Les hommes <30 ans bénéficient d'un seuil réduit de 1 terme.

## ??? Développement

### Prérequis

- .NET 9 SDK
- Visual Studio 2022 ou VS Code
- Docker Desktop (pour les tests d'intégration)

### Lancement en local

```bash
dotnet run --project Assessments_backend
```

L'API sera disponible sur `http://localhost:5000` (ou le port configuré).

## ?? TODO

- [ ] Implémenter le microservice Notes Backend
- [ ] Ajouter des tests d'intégration
- [ ] Configurer Serilog pour Graylog
- [ ] Ajouter des métriques (Prometheus)
- [ ] Ajouter un cache Redis pour les évaluations
- [ ] Implémenter le pattern Circuit Breaker (Polly)

## ?? Intégration avec Ocelot

Voir le fichier [OCELOT_CONFIGURATION.md](./OCELOT_CONFIGURATION.md) pour la configuration complète de l'API Gateway.

## ?? Licence

Projet réalisé à des fins pédagogiques dans le cadre du parcours OpenClassrooms.
