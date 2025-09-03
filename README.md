# 0.0.O 
# premier push ou

**Plan de tests TimeStock**

Le plan de tests de TimeStock répond à trois objectifs :
1. **Couverture unitaire** : garantir la fiabilité de la logique et des règles métiers (JWT, helpers, DTO).
2. **Intégration API-DB** : valider les interactions entre les contrôleurs, services et la base MySQL via Testcontainers.
3. **End-to-end (E2E) & infra** : simuler des parcours complets sur une stack Docker Compose (à venir).

### 1. Tests unitaires
- **Portée** : classes `JwtService`, helpers de validation, sérialisation des DTO.  
- **Outils** : xUnit, FluentAssertions, Moq si besoin.  
- **Exemples de cas** :  
  - Génération/validation de JWT (presence/expiration des claims)  
  - Round-trip JSON pour les DTO  
  - Validation de mots de passe via BCrypt  

### 2. Tests d’intégration (base réelle)
- **Portée** : appels HTTP réels contre l’API hébergée en mémoire (WebApplicationFactory) et connectée à un conteneur MySQL éphémère.  
- **Outils** :  
  - Microsoft.AspNetCore.Mvc.Testing (WebApplicationFactory)  
  - DotNet.Testcontainers (MySqlTestcontainer)  
  - FluentAssertions  
- **Scénarios clés** :  
  1. **Register → Login** (200 OK + JWT valide)  
  2. **/api/auth/me** sans JWT → 401  
  3. **/api/auth/me** avec JWT → 200 + données utilisateur  
  4. **Register duplicate** → 400 BadRequest  

### 3. Tests end-to-end & infrastructure (futur)
- **Portée** : démarrage de la stack Docker Compose, exécution de scripts Postman/Newman sur l’URL `/auth`, `/`, et routes privées.  
- **Outils** : Newman, Bash, GitHub Actions service containers.  
- **Objectif** : garantir que l’application packagée (server + client + db) fonctionne en condition réelle.

---

**Couverture & Reporting**

- **Collecteur** : Coverlet (coverlet.collector)  
- **Format** : Cobertura (`coverage.cobertura.xml`)  
- **Intégration** :  
  - Affichage en local via reportgenerator  
  - Publication sur GitHub Actions et analyse SonarCloud  
- **Seuils visés** :  
  - >90% sur les classes de sécurité (JwtService, DatabaseService)  
  - >80% sur le module Auth / multi-tenant  
