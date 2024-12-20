# SDK stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /src
COPY ./ .
RUN dotnet restore "./WebsiteAnalyzer.Web/WebsiteAnalyzer.Web.csproj" \
    && dotnet publish "./WebsiteAnalyzer.Web/WebsiteAnalyzer.Web.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Set environment variables
ENV DOTNET_gcServer=1 \
    DOTNET_GCHeapCount=2 \
    DOTNET_SYSTEM_NET_HTTP_SOCKETSHTTPHANDLER_HTTP3SUPPORT=1 \
    ASPNETCORE_URLS="https://+:8081;http://+:8080" \
    ASPNETCORE_HTTPS_PORT=8081

# Performance optimizations and QUIC installation
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        curl \
        wget \
        gnupg \
        libc6 \
    && wget -O - https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /usr/share/keyrings/microsoft-archive-keyring.gpg \
    && echo "deb [arch=amd64 signed-by=/usr/share/keyrings/microsoft-archive-keyring.gpg] https://packages.microsoft.com/debian/12/prod bookworm main" > /etc/apt/sources.list.d/microsoft.list \
    && apt-get update \
    && apt-get install -y --no-install-recommends \
        libmsquic \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /Data \
    && mkdir -p /https

# Copy from build
COPY --from=build-env /app/publish .

# Create directory for certificates
VOLUME ["/Data", "/https"]
EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "WebsiteAnalyzer.Web.dll"]