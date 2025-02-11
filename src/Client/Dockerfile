# Étape 1 : Build de l'application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o out

# Étape 2 : Serveur Nginx pour héberger Blazor
FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY --from=build /app/out/wwwroot .

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
