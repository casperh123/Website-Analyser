using System.Net;
using AngleSharp.Html.Parser;
using BrokenLinkChecker.crawler;
using BrokenLinkChecker.Crawler.ExtendedCrawlers;
using BrokenLinkChecker.DocumentParsing.ModularLinkExtraction;
using BrokenLinkChecker.Models.Links;
using BrokenLinkChecker.utility;

namespace BrokenLinkChecker.DocumentParsing.LinkProcessors;

public class BrokenLinkProcessor : ILinkProcessor<IndexedLink>
{
    private readonly Dictionary<string, HttpStatusCode> _visitedResources = new();
    private readonly HttpClient _httpClient;
    private AbstractLinkExtractor<IndexedLink> _linkExtractor; 
    
    public BrokenLinkProcessor(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _linkExtractor = new IndexedLinkExtractor(new HtmlParser(
                new HtmlParserOptions
                {
                    IsKeepingSourceReferences = true
                }
            )
        );
    } 
    
    public async Task<IEnumerable<IndexedLink>> ProcessLinkAsync(IndexedLink link, ModularCrawlResult<IndexedLink> crawlResult)
    {
        IEnumerable<IndexedLink> links = [];
        
        if (_visitedResources.TryGetValue(link.Target, out HttpStatusCode statusCode))
        {
            link.StatusCode = statusCode;
        }
        else
        {
            HttpResponseMessage response = await _httpClient.GetAsync(link.Target);
            _visitedResources[link.Target] = response.StatusCode;
            link.StatusCode = response.StatusCode;
            
            links = await _linkExtractor.GetLinksFromDocument(response, link);
        }
        
        crawlResult.IncrementLinksChecked();

        if (link.StatusCode is not HttpStatusCode.OK)
        {
            crawlResult.AddResource(link);
        }

        return links;
    }
}