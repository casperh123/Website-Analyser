    using WebsiteAnalyser.Api.Configuration;
    using WebsiteAnalyser.Api.Endpoints;
    using WebsiteAnalyzer.Infrastructure.Data.Configurations;
    using WebsiteAnalyzer.Infrastructure.DependencyInjection;

    var builder = WebApplication.CreateSlimBuilder(args);

    builder.ConfigureServer();

    builder.Configuration.AddEnvironmentVariables();

    builder.Services
        .AddHttpClients()
        .AddDatabaseServices(builder.Configuration)
        .AddAuthenticationServices()
        .AddInfrastructureServices(builder.Configuration)
        .AddApplicationServices();

    builder.Services.AddLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();  // For console output
        logging.AddDebug();    // For debug window output
        
        logging.SetMinimumLevel(LogLevel.Information);
    });


    // Build and configure the application
    WebApplication app = builder.Build();

    await DatabaseMigrator.MigrateDatabase(app);

    // Configure the HTTP pipeline
    app.ConfigurePipeline();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.MapWebsiteEndpoints();
    app.MapBrokenLinkEndpoints();
    app.MapCacheWarmingEndpoints();
    app.MapUptimeEndpoints();
    app.MapEmailSubscriptionEndpoints();

    app.Run();
