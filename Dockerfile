# syntax=docker/dockerfile:1

# One image, three stages: Node builds the client, the .NET SDK builds the API, and the
# runtime image receives both. The client is not a separate service because it is not a
# server — it is a folder of static files, and the API can serve a folder.

FROM node:22-alpine AS client
WORKDIR /client

# Lockfile first for the same reason as the csproj files below: `npm ci` is the slow
# layer and it depends on nothing else, so editing a .tsx file reuses it.
COPY client/package.json client/package-lock.json ./
RUN npm ci

COPY client/ ./

# Empty on purpose. The API serves this bundle, so every request the client makes is
# same-origin and needs no host in front of it. Overridable if the client is ever hosted
# apart from the API.
ARG VITE_API_BASE_URL=""
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL

RUN npm run build


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

# The client build lands where ASP.NET already looks for static content. Nothing in the
# API is aware this folder came from a different toolchain.
COPY --from=client /client/dist ./wwwroot

# The runtime images ship a non-root account for exactly this. Running as root inside a
# container that only needs to read its own files buys nothing.
USER $APP_UID

# ASPNETCORE_HTTP_PORTS defaults to 8080 in these images, and the app leaves HTTPS to
# whatever terminates it, so nothing here needs configuring for the container to serve.
EXPOSE 8080

ENTRYPOINT ["dotnet", "VehicleExplorer.Api.dll"]
