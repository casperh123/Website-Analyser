#!/bin/bash

# Remove existing migrations and database
rm -rf ./WebsiteAnalyzer.Infrastructure/Migrations
rm -rf ./WebsiteAnalyzer.Web/Data/app.db

# Create empty database directory if it doesn't exist
mkdir -p ./WebsiteAnalyzer.Web/Data
# Create empty database
sqlite3 ./WebsiteAnalyzer.Web/Data/app.db ".quit"

# Create tool manifest (toolbox) if it doesn't exist
if [ ! -f ".config/dotnet-tools.json" ]; then
    echo "Creating new tool manifest..."
    dotnet new tool-manifest
fi

# Install EF Core tool to the local toolbox if not already installed
if ! dotnet tool list | grep -q "dotnet-ef"; then
    echo "Installing ef tool to local manifest..."
    dotnet tool install dotnet-ef
fi

# Restore tools from manifest
echo "Restoring tools from manifest..."
dotnet tool restore

# Generate and apply migrations
echo "Generating new migration..."
dotnet ef migrations add MigrationName --startup-project ./WebsiteAnalyzer.Web/WebsiteAnalyzer.Web.csproj --project ./WebsiteAnalyzer.Infrastructure/WebsiteAnalyzer.Infrastructure.csproj

echo "Updating database..."
dotnet ef database update --startup-project ./WebsiteAnalyzer.Web/WebsiteAnalyzer.Web.csproj --project ./WebsiteAnalyzer.Infrastructure/WebsiteAnalyzer.Infrastructure.csproj