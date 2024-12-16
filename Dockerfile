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
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV COMPlus_EnableDiagnostics=0
ENV DOTNET_gcServer=1
ENV DOTNET_GCHeapCount=2

# Performance optimizations
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl libc6 \
    && rm -rf /var/lib/apt/lists/*

# Copy from build
COPY --from=build-env /app/publish .
RUN mkdir -p /Data && chown -R $APP_UID:$APP_UID /Data

USER $APP_UID
EXPOSE 8080
VOLUME ["/Data"]
ENTRYPOINT ["dotnet", "WebsiteAnalyzer.Web.dll"]