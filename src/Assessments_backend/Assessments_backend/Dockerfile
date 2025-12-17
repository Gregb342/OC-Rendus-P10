FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Assessments_backend.csproj", "."]
RUN dotnet restore "./Assessments_backend.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Assessments_backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Assessments_backend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Assessments_backend.dll"]
