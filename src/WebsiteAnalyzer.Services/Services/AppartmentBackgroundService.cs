using System.Net.Http.Json;
using WebsiteAnalyzer.Application.Services;
using WebsiteAnalyzer.Services.Cache;
using WebsiteAnalyzer.Web.BackgroundJobs.Timers;

namespace WebsiteAnalyzer.Services.Services;

public class AppartmentBackgroundService : BackgroundService
{
    private readonly IPeriodicTimer _timer;
    private readonly IServiceProvider _serviceProvider;
    private readonly AppartmentService _appartmentService;
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
        HttpClient client = _serviceProvider.GetRequiredService<HttpClient>();
        MailService mailService = _serviceProvider.GetRequiredService<MailService>();
        lastCheck = DateTime.Now;
        _appartmentService = new AppartmentService(client, mailService);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && await _timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await _appartmentService.GetAppartments();
            }
            catch
            {
            }
        }
    }
}

