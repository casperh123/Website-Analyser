#!/bin/bash

# Define paths
DB_DIR="./WebsiteAnalyzer.Web/Data"
APP_DB_FILE="$DB_DIR/app.db"
IDENTITY_DB_FILE="$DB_DIR/identity.db"  # You can use the same path as app.db if they share a database

# Remove existing migrations and databases
rm -rf ./WebsiteAnalyzer.Infrastructure/Migrations
rm -f "$APP_DB_FILE"
rm -f "$IDENTITY_DB_FILE"  # Remove if using separate files

# Create empty database directory if it doesn't exist
mkdir -p "$DB_DIR"

# Create empty databases
sqlite3 "$APP_DB_FILE" ".quit"
sqlite3 "$IDENTITY_DB_FILE" ".quit"  # Create if using separate files

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

# Generate and apply migrations for Application DbContext
echo "Generating new migration for Application context..."
dotnet ef migrations add ApplicationMigration \
    --context ApplicationDbContext \
    --output-dir Migrations/ApplicationDb \
    --startup-project ./WebsiteAnalyzer.Web/WebsiteAnalyzer.Web.csproj \
    --project ./WebsiteAnalyzer.Infrastructure/WebsiteAnalyzer.Infrastructure.csproj

echo "Updating Application database..."
dotnet ef database update \
    --context ApplicationDbContext \
    --startup-project ./WebsiteAnalyzer.Web/WebsiteAnalyzer.Web.csproj \
    --project ./WebsiteAnalyzer.Infrastructure/WebsiteAnalyzer.Infrastructure.csproj

# Generate and apply migrations for Identity DbContext
# Make sure to use the exact context name you registered
echo "Generating new migration for Identity context..."
dotnet ef migrations add IdentityMigration \
    --context ApplicationIdentityDbContext \
    --output-dir Migrations/IdentityDb \
    --startup-project ./WebsiteAnalyzer.Web/WebsiteAnalyzer.Web.csproj \
    --project ./WebsiteAnalyzer.Infrastructure/WebsiteAnalyzer.Infrastructure.csproj

echo "Updating Identity database..."
dotnet ef database update \
    --context ApplicationIdentityDbContext \
    --startup-project ./WebsiteAnalyzer.Web/WebsiteAnalyzer.Web.csproj \
    --project ./WebsiteAnalyzer.Infrastructure/WebsiteAnalyzer.Infrastructure.csproj

echo "Migration process completed!"