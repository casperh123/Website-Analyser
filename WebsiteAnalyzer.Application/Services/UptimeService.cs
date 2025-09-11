using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;

namespace WebsiteAnalyzer.Application.Services;

public class UptimeService : IUptimeService
{
    private IUptimeRepository _uptimeRepository;

    public UptimeService(IUptimeRepository uptimeRepository)
    {
        _uptimeRepository = uptimeRepository;
    }
}