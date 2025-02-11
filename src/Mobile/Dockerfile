# Étape 1 : Utiliser le SDK .NET avec le support MAUI
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Installer le workload MAUI Android
RUN dotnet workload install maui-android

# Copier les fichiers et restaurer les dépendances
COPY . ./
RUN dotnet restore

# Compiler l'application pour Android
RUN dotnet publish -c Release -f net8.0-android -o /out

# Étape 2 : Stocker les fichiers compilés
FROM busybox:latest
WORKDIR /app
COPY --from=build /out .

CMD ["echo", "Build MAUI Android terminé !"]
