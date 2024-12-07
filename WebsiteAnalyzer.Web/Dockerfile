# Syntax version
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# Copy csproj file and restore dependencies first (layer caching optimization)
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the files and build
COPY . ./
RUN dotnet publish -c Release -o out --no-restore /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Create and set permissions for Data directory mount point
RUN mkdir -p /Data && chmod 777 /Data

# Copy built application
COPY --from=build-env /app/out .

# Mount point for Data directory
VOLUME ["/Data"]

ENTRYPOINT ["dotnet", "WebsiteAnalyser.Web.dll"]
