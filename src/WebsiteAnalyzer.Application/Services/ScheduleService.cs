using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Core.Interfaces.Services.DTOs.ScheduledAction;

namespace WebsiteAnalyzer.Application.Services;


public class ScheduleService : IScheduleService
{
    private readonly IScheduledActionRepository _scheduleRepository;
    private readonly IWebsiteRepository _websiteRepository;

    public ScheduleService(
        IScheduledActionRepository scheduleRepository,
        IWebsiteRepository websiteRepository
        )
    {
        _scheduleRepository = scheduleRepository;
        _websiteRepository = websiteRepository;
    }

    public async Task<ScheduledActionDTO> ScheduleAction(
        Guid websiteId,
        CrawlAction action,
        Frequency frequency,
        TimeSpan negativeOffset = default)
    {
        Website website = await _websiteRepository.GetByWebsiteId(websiteId);
        ScheduledAction scheduledAction = new ScheduledAction(
            website,
            frequency,
            action,
            negativeOffset
        );

        await _scheduleRepository.AddAsync(scheduledAction);

        return ScheduledActionDTO.From(scheduledAction);
    }

    public async Task DeleteAction(Guid actionId)
    {
        ScheduledAction? action = await _scheduleRepository.GetByIdAsync(actionId);

        if (action is null)
        {
            //TODO return a not found error of sorts
            return;
        }
        
        await _scheduleRepository.DeleteAsync(action);
    }

    public async Task<ScheduledActionDTO> GetById(Guid id)
    {
        //TODO change to proper not found exception
        ScheduledAction action = await _scheduleRepository.GetByIdAsync(id) ?? throw new InvalidOperationException();
        return ScheduledActionDTO.From(action);
    }

    public async Task<ICollection<ScheduledActionDTO>> GetByWebsiteId(Guid websiteId)
    {
        var actions = await _scheduleRepository.GetByWebsiteId(websiteId);
        return [.. actions.Select(ScheduledActionDTO.From)];
    }

    public async Task<ICollection<ScheduledActionDTO>> GetByWebsiteIds(ICollection<Guid> websiteIds)
    {
        var actions = await _scheduleRepository.GetByWebsiteIds(websiteIds);
        return [.. actions.Select(ScheduledActionDTO.From)];
    }

    public async Task<ScheduledActionDTO?> GetActionByWebsiteIdAndType(
        Guid websiteId,
        CrawlAction type)
    {
        var action = await _scheduleRepository.GetByWebsiteIdAndType(websiteId, type);

        return action is null
            ? null
            : ScheduledActionDTO.From(action);
    }

    public async Task<ICollection<ScheduledActionDTO>> GetScheduledTasksByUserIdAndTypeAsync(Guid? userId, CrawlAction action)
    {
        if (!userId.HasValue)
        {
            return [];
        }

        ICollection<ScheduledAction> actions =
            await _scheduleRepository.GetCrawlSchedulesByUserIdAndTypeAsync(userId.Value, action);
        
        return [..actions.Select(ScheduledActionDTO.From)];
    }

    public async Task DeleteTasksByUrlAndUserId(string url, Guid userId)
    {
        await _scheduleRepository.DeleteByUrlAndUserId(url, userId);
    }

    public async Task UpdateStatus(Guid actionId, Status status)
    {
        ScheduledAction? action = await _scheduleRepository.GetByIdAsync(actionId);

        if (action is null)
        {
            return;
        }
        
        action.Status = status;

        await _scheduleRepository.UpdateAsync(action);
    }

    public async Task ResetActionStatus(Guid actionId)
    {
        ScheduledAction? action = await _scheduleRepository.GetByIdAsync(actionId);

        if (action is null)
        {
            return;
        }
        
        action.ResetStatus();

        await _scheduleRepository.UpdateAsync(action);
    }

    public async Task<ICollection<ScheduledActionDTO>> GetDueSchedulesBy(CrawlAction action)
    {
        ICollection<ScheduledAction> schedules = await _scheduleRepository.GetByAction(action);

        return
        [
            .. schedules
                .Where(cs => cs.IsDueForExecution)
                .Select(ScheduledActionDTO.From)
        ];
    }

    public async Task StartAction(Guid actionId)
    {
        ScheduledAction? action = await _scheduleRepository.GetByIdAsync(actionId);
        
        action?.StartAction();

        await UpdateAction(action);
    }

    public async Task CompleteAction(Guid actionId)
    {
        ScheduledAction? action = await _scheduleRepository.GetByIdAsync(actionId);
        
        action?.CompleteAction();

        await UpdateAction(action);
    }

    public async Task FailAction(Guid actionId)
    {
        ScheduledAction? action = await _scheduleRepository.GetByIdAsync(actionId);
        
        action?.FailAction();

        await UpdateAction(action);
    }

    private async Task UpdateAction(ScheduledAction? action)
    {
        if (action is null)
        {
            return;
        }
        
        await _scheduleRepository.UpdateAsync(action);
    }
}