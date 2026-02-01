using System.Net.Http.Json;
using System.Threading.Tasks.Dataflow;
using MailKit;
using Microsoft.AspNetCore.Identity.UI.Services;
using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Web.BackgroundJobs.Timers;

namespace WebsiteAnalyzer.Services.Services;

public class AppartmentBackgroundService : BackgroundService
{
    private readonly IPeriodicTimer _timer;
    private readonly HttpClient _client;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEmailSender _mailService;
    protected readonly ILogger Logger;
    private DateTime lastCheck;

    public AppartmentBackgroundService(
        ILogger<AppartmentBackgroundService> logger,
        IServiceProvider serviceprovider
        )
    {
        _serviceProvider = serviceprovider;
        _timer = new MinuteTimer(1);
        Logger = logger;
        _client = _serviceProvider.GetRequiredService<HttpClient>();
        _mailService = _serviceProvider.GetRequiredService<IEmailSender>();
        lastCheck = DateTime.Now;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && await _timer.WaitForNextTickAsync(stoppingToken))
        {
            JoratoResponse? response = await _client.GetFromJsonAsync<JoratoResponse>("https://api.jorato.com/tenancies?visibility=public&showAll=true&key=2gXoBtKvFMMgKJ1VBJ5G5pNr2GD", cancellationToken: stoppingToken);

            IEnumerable<TenancyDto> urls = response.items
                .Where(s => s.available);

            if (true)
            {
                await _mailService.SendEmailAsync("clypper.tech@protonmail.com", "Kereby Lejlighed Tilgængelig", "Der er kommet nye boliger");
                await _mailService.SendEmailAsync("ie@live.dk", "Kereby Lejlighed Tilgængelig", "Der er kommet nye boliger");
            }
        }
    }
}

public class JoratoResponse
{
    public List<TenancyDto> items { get; set; } = [];
}

public class TenancyDto
{
    public bool available { get; set; }

    public string url { get; set; } = string.Empty;
}

