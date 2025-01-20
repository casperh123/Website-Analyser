using WebsiteAnalyzer.Infrastructure.Data.Configurations;
using WebsiteAnalyzer.Infrastructure.DependencyInjection;
using WebsiteAnalyzer.Web.Configuration;
using WebsiteAnalyzer.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure the server settings (Kestrel, etc.)
builder.ConfigureServer();

// Add core Blazor services
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services
    .AddHttpClients()
    .AddDatabaseServices(builder.Configuration)
    .AddAuthenticationServices()
    .AddInfrastructureServices(builder.Configuration)
    .AddApplicationServices()
    .AddThemeServices()
    .AddBackgroundServices();

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();  // For console output
    logging.AddDebug();    // For debug window output
    
    logging.SetMinimumLevel(LogLevel.Information);
});

// Build and configure the application
var app = builder.Build();

// Run database migrations
await DatabaseMigrator.MigrateDatabase(app);

// Configure the HTTP pipeline
app.ConfigurePipeline();

// Start the application
app.Run();