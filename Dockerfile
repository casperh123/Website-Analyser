# Syntax version
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /WebsiteAnalyzer.Web
# Copy the rest of the files and build
COPY ./ ./
RUN dotnet restore ./WebsiteAnalyzer.Web/WebsiteAnalyzer.Web.csproj
RUN dotnet publish ./WebsiteAnalyzer.Web/WebsiteAnalyzer.Web.csproj -c Release -o out --no-restore /p:UseAppHost=false
# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /WebsiteAnalyzer.Web
# Create and set permissions for Data directory mount point
RUN mkdir -p /Data && chmod 777 /Data
# Copy built application
COPY --from=build-env /WebsiteAnalyzer.Web/out .
# Mount point for Data directory
VOLUME ["/Data"]
ENTRYPOINT ["dotnet", "WebsiteAnalyzer.Web.dll"]
