# SDK stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build-env
WORKDIR /src
COPY ./src .

RUN dotnet restore "WebsiteAnalyzer.Services/WebsiteAnalyzer.Services.csproj" \
    && dotnet publish "WebsiteAnalyzer.Services/WebsiteAnalyzer.Services.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble

WORKDIR /app

COPY --from=build-env /app/publish .

# Install libmsquic for HTTP/3 support
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        curl \
        wget \
        gnupg \
    && wget -O - https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /usr/share/keyrings/microsoft-archive-keyring.gpg \
    && echo "deb [arch=amd64 signed-by=/usr/share/keyrings/microsoft-archive-keyring.gpg] https://packages.microsoft.com/ubuntu/24.04/prod noble main" > /etc/apt/sources.list.d/microsoft.list \
    && apt-get update \
    && apt-get install -y --no-install-recommends libmsquic \
    && rm -rf /var/lib/apt/lists/*

ENV DOTNET_gcServer=0 \
    ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "WebsiteAnalyzer.Services.dll"]