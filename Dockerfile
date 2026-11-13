FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY StoreApi/StoreApi.csproj StoreApi/
COPY StoreApi.Infra/StoreApi.Infra.csproj StoreApi.Infra/
RUN dotnet restore StoreApi/StoreApi.csproj
COPY StoreApi/ StoreApi/
COPY StoreApi.Infra/ StoreApi.Infra/
RUN dotnet publish StoreApi/StoreApi.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
USER app
EXPOSE 10000
ENTRYPOINT ["dotnet", "StoreApi.dll"]
