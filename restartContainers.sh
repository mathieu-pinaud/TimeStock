#!/bin/bash

# Vérification de Docker
if ! command -v docker &> /dev/null; then
    echo "Docker n'est pas installé. Veuillez l'installer."
    exit 1
fi

# Aller dans le dossier Docker
cd infrastructure/Docker || { echo "Dossier infrastructure/Docker introuvable."; exit 1; }

# Arrêt des conteneurs
docker-compose down

# Nettoyage des ressources inutilisées
docker system prune -f --volumes

# Reconstruction et redémarrage des conteneurs
docker-compose up --build -d

# Affichage des conteneurs en cours d'exécution
docker ps
