#!/bin/bash

# Vérification des paramètres
if [ "$#" -ne 1 ]; then
  echo "Usage: $0 <branche-sous-module>"
  echo "Exemple: $0 master"
  exit 1
fi

SUBMODULE_BRANCH=$1  # Branche du sous-module (ex: master)

# Vérifier si on est bien dans TimeStock
if [ ! -d "infrastructure/Docker" ]; then
  echo "Erreur : Ce script doit être exécuté depuis le dossier TimeStock."
  exit 1
fi

echo "Récupération des mises à jour du sous-module TimeStock-Docker..."

# Aller dans le sous-module et récupérer les dernières modifications
cd infrastructure/Docker || exit
git checkout $SUBMODULE_BRANCH
git pull origin $SUBMODULE_BRANCH

# Revenir dans le repo principal et mettre à jour le submodule
cd ../..
git submodule update --remote --merge

echo "Mises à jour du sous-module téléchargées. Aucun commit/push n'a été effectué."