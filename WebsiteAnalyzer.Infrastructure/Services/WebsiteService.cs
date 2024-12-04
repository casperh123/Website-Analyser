using AngleSharp.Dom;
using BrokenLinkChecker.crawler;

namespace WebsiteAnalyzer.Infrastructure.Services;

public interface IWebsiteService
{
    Task AddWebsiteToUser(string url, Crawler crawler, ApplicationUser user);
}

public class WebsiteService
{
    
}