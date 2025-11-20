using WebsiteAnalyzer.Infrastructure.Data.Configurations;
using WebsiteAnalyzer.Infrastructure.DependencyInjection;
using WebsiteAnalyzer.Services.Configuration;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddDefaultHttpClient()
    .AddDatabaseServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddApplicationServices()
    .AddBackgroundServices();

IHost host = builder.Build();
host.Run();