# Build multi-stage: compila con el SDK de .NET y corre con el runtime ASP.NET
# (más liviano). Se construye en cualquier máquina con internet o en
# GitHub Actions — ver .github/workflows/backend-ci.yml.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/FyaCreditos.Api/FyaCreditos.Api.csproj ./FyaCreditos.Api/
RUN dotnet restore ./FyaCreditos.Api/FyaCreditos.Api.csproj

COPY src/FyaCreditos.Api/. ./FyaCreditos.Api/
WORKDIR /src/FyaCreditos.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FyaCreditos.Api.dll"]
