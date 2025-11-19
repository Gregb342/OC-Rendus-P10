# Étape 1 : Image de build (compilation)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copie du fichier projet et restauration des dépendances
COPY ["Patients-Frontend.csproj", "./"]
RUN dotnet restore "Patients-Frontend.csproj"

# Copie de tout le code source
COPY . .

# Compilation en mode Release
RUN dotnet build "Patients-Frontend.csproj" -c Release -o /app/build

# Publication de l'application
FROM build AS publish
RUN dotnet publish "Patients-Frontend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Étape 2 : Image finale d'exécution (plus légère)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Exposition du port 8080
EXPOSE 8080
EXPOSE 8081

# Copie des fichiers publiés
COPY --from=publish /app/publish .

# Point d'entrée de l'application
ENTRYPOINT ["dotnet", "Patients-Frontend.dll"]