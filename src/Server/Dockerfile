# Dockerfile pour le backend (ASP.NET Core API)

# Étape 1 : Construction de l'application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copier les fichiers projet et restaurer les dépendances
COPY ./*.csproj ./
RUN dotnet restore

# Copier tous les fichiers et publier l'application
COPY . ./
RUN dotnet publish -c Release -o /out

# Étape 2 : Image finale pour exécution
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copier l'application depuis l'étape de build
COPY --from=build /out .

# Exposer les ports utilisés par l'API
EXPOSE 5000 5001

# Définir la commande de démarrage
CMD ["dotnet", "Server.dll"]
