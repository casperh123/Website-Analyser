using Microsoft.AspNetCore.Identity.UI.Services;
using Radzen;
using Sidio.Sitemap.Blazor;
using Sidio.Sitemap.Core.Services;
using WebsiteAnalyzer.Infrastructure.Data.Configurations;
using WebsiteAnalyzer.Infrastructure.DependencyInjection;
using WebsiteAnalyzer.Web.Configuration;
using WebsiteAnalyzer.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure the server settings (Kestrel, etc.)
builder.ConfigureServer();

builder.Configuration.AddEnvironmentVariables();

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
    .AddHttpContextAccessor()
    .AddDefaultSitemapServices<HttpContextBaseUrlProvider>();

builder.Services.AddTransient<IEmailSender, MailSenderProvider>();

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<StateService>();

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();  // For console output
    logging.AddDebug();    // For debug window output
    
    logging.SetMinimumLevel(LogLevel.Information);
});

builder.Services.AddRadzenComponents();

builder.Services.AddScoped<Radzen.DialogService>();



// Build and configure the application
WebApplication app = builder.Build();

// Run database migrations
await DatabaseMigrator.MigrateDatabase(app);

// Configure the HTTP pipeline
app.ConfigurePipeline();

app.UseSitemap();

// Start the application
app.Run();