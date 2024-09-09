using BrokenLinkChecker.Crawler.ExtendedCrawlers;
using BrokenLinkChecker.models.Links;
using BrokenLinkChecker.Models.Links;

namespace BrokenLinkChecker.DocumentParsing;

public interface ILinkProcessor<T> where T : NavigationLink
{
    public Task<List<T>> ProcessLinkAsync(T link, ModularCrawlResult<T> crawlResult);
}