using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Enums;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IScheduleService
{
    Task<CrawlSchedule> ScheduleTask(string url, Guid userId, CrawlAction action, Frequency frequency);
    Task UpdateScheduledTask(CrawlSchedule scheduledTask, Frequency frequency);
    Task DeleteScheduledTask(CrawlSchedule scheduledTask);
    Task<ICollection<CrawlSchedule>> GetScheduledTasksByUserIdAndTypeAsync(Guid? userId, CrawlAction action);
    Task DeleteTasksByUrlAndUserId(string url, Guid userId);
}