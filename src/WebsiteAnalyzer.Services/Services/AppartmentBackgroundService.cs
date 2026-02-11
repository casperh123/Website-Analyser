using System.Net.Http.Json;
using WebsiteAnalyzer.Application.Services;
using WebsiteAnalyzer.Services.Cache;
using WebsiteAnalyzer.Web.BackgroundJobs.Timers;

namespace WebsiteAnalyzer.Services.Services;

public class AppartmentBackgroundService : BackgroundService
{
    private readonly IPeriodicTimer _timer;
    private readonly HttpClient _client;
    private readonly IServiceProvider _serviceProvider;
    private readonly MailService _mailService;
    protected readonly ILogger Logger;
    private DateTime lastCheck;
    private AppartmentCache _cache = new AppartmentCache();
    
    

    public AppartmentBackgroundService(
        ILogger<AppartmentBackgroundService> logger,
        IServiceProvider serviceprovider
        )
    {
        _serviceProvider = serviceprovider;
        _timer = new MinuteTimer(1);
        Logger = logger;
        _client = _serviceProvider.GetRequiredService<HttpClient>();
        _mailService = _serviceProvider.GetRequiredService<MailService>();
        lastCheck = DateTime.Now;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && await _timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                JoratoResponse? response = await _client.GetFromJsonAsync<JoratoResponse>("https://api.jorato.com/tenancies?visibility=public&showAll=true&key=2gXoBtKvFMMgKJ1VBJ5G5pNr2GD", cancellationToken: stoppingToken);

                IEnumerable<TenancyDto> urls = response.items
                    .Where(IsAvailable)
                    .Where(IsResidential);
                
                IEnumerable<TenancyDto> unseen = urls.Where(s => !_cache.Exists(s.id));

                if (unseen.Any())
                {
                    try
                    {
                        await _mailService.SendEmailAsync("clypper.tech@protonmail.com", "Kereby Lejlighed Tilgængelig",
                            "Der er kommet nye boliger");
                        await _mailService.SendEmailAsync("ie@live.dk", "Kereby Lejlighed Tilgængelig",
                            "Der er kommet nye boliger.");
                    }
                    catch
                    {
                        
                    }
                }
                
                _cache.AddAppartments(urls.Select(s => s.id));
                Console.WriteLine("Hey there");
            }
            catch
            {
            }
        }
    }

    public bool IsAvailable(TenancyDto appartment) => appartment.state switch
    {
        "Available" => true,
        _ => false
    };

    public bool IsResidential(TenancyDto tenancy) => tenancy.classification switch
    {
        "Residential" => true,
        _ => false
    };
}

public class JoratoResponse
{
    public List<TenancyDto> items { get; set; } = [];
}

public class TenancyDto
{
    public string state { get; set; } = string.Empty;
    public string classification { get; set; } = string.Empty;

    public string url { get; set; } = string.Empty;
    public string id { get; set; } = string.Empty;
}

