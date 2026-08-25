# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Project files first, restore, then the rest of the source. Restore is the slow layer and
# it only depends on these, so editing a .cs file reuses the cached packages instead of
# fetching them again.
#
# Directory.Build.props carries TargetFramework for every project, so it has to be in
# place before restore runs or the projects have no framework to restore against.
COPY Directory.Build.props ./
COPY src/VehicleExplorer.Api/VehicleExplorer.Api.csproj src/VehicleExplorer.Api/
COPY src/VehicleExplorer.Application/VehicleExplorer.Application.csproj src/VehicleExplorer.Application/
COPY src/VehicleExplorer.Infrastructure/VehicleExplorer.Infrastructure.csproj src/VehicleExplorer.Infrastructure/

RUN dotnet restore src/VehicleExplorer.Api/VehicleExplorer.Api.csproj

COPY src/ src/

RUN dotnet publish src/VehicleExplorer.Api/VehicleExplorer.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app ./

# The runtime images ship a non-root account for exactly this. Running as root inside a
# container that only needs to read its own files buys nothing.
USER $APP_UID

# ASPNETCORE_HTTP_PORTS defaults to 8080 in these images, and the app leaves HTTPS to
# whatever terminates it, so nothing here needs configuring for the container to serve.
EXPOSE 8080

ENTRYPOINT ["dotnet", "VehicleExplorer.Api.dll"]
