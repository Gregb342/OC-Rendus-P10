FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Patients.csproj", "."]
RUN dotnet restore "./Patients.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Patients.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Patients.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Patients.dll"]