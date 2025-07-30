#!/bin/bash

# Vérification des paramètres
if [ "$#" -ne 1 ]; then
  echo "Usage: $0 <branche-sous-module>"
  echo "Exemple: $0 master"
  exit 1
fi

SUBMODULE_BRANCH=$1  # Branche du sous-module (ex: master)

# Vérifier si on est bien dans TimeStock
if [ ! -d "infrastructure" ]; then
  echo "Erreur : Ce script doit être exécuté depuis le dossier racine du projet TimeStock."
  exit 1
fi

# Initialiser le sous-module s'il n'existe pas encore
if [ ! -d "infrastructure/Docker/.git" ]; then
  echo "Initialisation du sous-module TimeStock-Docker..."
  git submodule update --init --recursive
else
  echo "Sous-module déjà initialisé."
fi

# Aller dans le sous-module et basculer sur la branche voulue
echo "Mise à jour du sous-module TimeStock-Docker (branche '$SUBMODULE_BRANCH')..."
cd infrastructure/Docker || exit 1
git checkout $SUBMODULE_BRANCH
git pull origin $SUBMODULE_BRANCH

# Revenir à la racine et mettre à jour le submodule dans le projet principal
cd ../..
git submodule update --remote --merge

echo "Sous-module mis à jour avec succès. Aucun commit/push n'a été effectué."