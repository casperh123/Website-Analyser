using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Entities.Website;
using WebsiteAnalyzer.Core.Enums;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IScheduleService
{
    Task<ScheduledAction> ScheduleTask(Website website, CrawlAction action, Frequency frequency, DateTime firstCrawl);
    Task UpdateScheduledTask(ScheduledAction scheduledTask, Frequency frequency);
    Task DeleteScheduledTask(ScheduledAction scheduledTask);
    Task<ICollection<ScheduledAction>> GetScheduledTasksByUserIdAndTypeAsync(Guid? userId, CrawlAction action);
    Task DeleteTasksByUrlAndUserId(string url, Guid userId);
}