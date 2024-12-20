using System.Net;
using System.Security.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Radzen;
using WebsiteAnalyzer.Core.Persistence;
using WebsiteAnalyzer.Infrastructure;
using WebsiteAnalyzer.Infrastructure.Data;
using WebsiteAnalyzer.Infrastructure.Repositories;
using WebsiteAnalyzer.Infrastructure.Services;
using WebsiteAnalyzer.Web.BackgroundJobs;
using WebsiteAnalyzer.Web.Components;
using WebsiteAnalyzer.Web.Components.Account;
using WebsiteAnalyzer.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Configure HTTP endpoint
    serverOptions.Listen(IPAddress.Any, 8080);
    
    // Configure HTTPS endpoint
    serverOptions.Listen(IPAddress.Any, 8081, listenOptions =>
    {
        listenOptions.UseHttps();
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2AndHttp3;
    });

    serverOptions.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
    });
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.AddHttpClient("WebsiteAnalyser", client =>
    {
        client.DefaultRequestVersion = HttpVersion.Version30;
        client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        client.Timeout = TimeSpan.FromSeconds(20);
        client.DefaultRequestHeaders.ConnectionClose = false;

        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        client.DefaultRequestHeaders.Accept.ParseAdd(
            "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
        client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
    {
        UseCookies = false,
        SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
        MaxConnectionsPerServer = 50,
        MaxAutomaticRedirections = 10,
        AutomaticDecompression = DecompressionMethods.All,
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

builder.Services.AddScoped<ThemeService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Services.AddScoped<IWebsiteRepository, WebsiteRepository>();
builder.Services.AddScoped<ICacheWarmRepository, CacheWarmRepository>();
builder.Services.AddScoped<ICrawlScheduleRepository, CrawlSheduleRepository>();

builder.Services.AddScoped<IWebsiteService, WebsiteService>();
builder.Services.AddScoped<ICacheWarmingService, CacheWarmingService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IBrokenLinkService, BrokenLinkService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddSingleton<IPeriodicTimer, HourlyTimer>();

builder.Services.Configure<HostOptions>(options => { options.ServicesStartConcurrently = true; });

builder.Services.AddHostedService<CacheWarmBackgroundService>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    IServiceProvider services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();