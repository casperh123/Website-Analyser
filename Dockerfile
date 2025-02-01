ARG NEW_RELIC_LICENSE_KEY

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

ARG NEW_RELIC_LICENSE_KEY

ENV \
    # Enable tiered compilation for better JIT performance
    COMPlus_TieredCompilation=1 \
    # Optimize for server garbage collection
    DOTNET_gcServer=1 \
    # Enable ready-to-run compilation
    DOTNET_ReadyToRun=1 \
    DOTNET_EnableDiagnostics=0 \
    ASPNETCORE_ENVIRONMENT=Production

WORKDIR /app

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

# Install the agent
RUN apt-get update && apt-get install -y wget ca-certificates gnupg \
  && echo 'deb http://apt.newrelic.com/debian/ newrelic non-free' | tee /etc/apt/sources.list.d/newrelic.list \
  && wget https://download.newrelic.com/548C16BF.gpg \
  && apt-key add 548C16BF.gpg \
  && apt-get update \
  && apt-get install -y 'newrelic-dotnet-agent' \
  && rm -rf /var/lib/apt/lists/* \

# Enable the agent
ENV CORECLR_ENABLE_PROFILING=1 \
    CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
    CORECLR_NEWRELIC_HOME=/usr/local/newrelic-dotnet-agent \
    CORECLR_PROFILER_PATH=/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so \
    NEW_RELIC_APP_NAME="Website Analyzer" \
    NEW_RELIC_LICENSE_KEY=${NEW_RELIC_LICENSE_KEY}


COPY --from=build-env /app/publish .
VOLUME ["/Data", "/https"]
EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "WebsiteAnalyzer.Web.dll"]