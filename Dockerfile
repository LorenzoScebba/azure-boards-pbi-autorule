# Create runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base-runtime

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app/

# Copy csproj
COPY src/azure-boards-pbi-autorule/*.csproj ./src/azure-boards-pbi-autorule/

# Restore
RUN cd src/azure-boards-pbi-autorule && dotnet restore --source "https://api.nuget.org/v3/index.json" && cd ..

# Copy everything else
COPY src/azure-boards-pbi-autorule ./src/azure-boards-pbi-autorule

WORKDIR /app/src/azure-boards-pbi-autorule

RUN dotnet publish -c Release -o out

# Build runtime image
FROM base-runtime AS final-runtime
WORKDIR /app
COPY --from=build-env /app/src/azure-boards-pbi-autorule/out .
ENTRYPOINT ["dotnet", "azure-boards-pbi-autorule.dll"]
