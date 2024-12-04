using System.Net;
using AngleSharp.Html.Parser;
using BrokenLinkChecker.Crawler.ExtendedCrawlers;
using BrokenLinkChecker.DocumentParsing.ModularLinkExtraction;
using BrokenLinkChecker.Models.Links;

namespace BrokenLinkChecker.DocumentParsing.LinkProcessors;

public class BrokenLinkProcessor : ILinkProcessor<IndexedLink>
{
    private readonly HttpClient _httpClient;
    private readonly AbstractLinkExtractor<IndexedLink> _linkExtractor;
    private readonly Dictionary<string, HttpStatusCode> _visitedResources = new();

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

    public async Task<IEnumerable<IndexedLink>> ProcessLinkAsync(IndexedLink link,
        ModularCrawlResult<IndexedLink> crawlResult)
    {
        IEnumerable<IndexedLink> links = Enumerable.Empty<IndexedLink>();

        if (!_visitedResources.TryGetValue(link.Target, out var statusCode))
            try
            {
                using var response =
                    await _httpClient.GetAsync(link.Target, HttpCompletionOption.ResponseHeadersRead);
                statusCode = response.StatusCode;
                _visitedResources[link.Target] = statusCode;

                if (response.IsSuccessStatusCode)
                    links = await _linkExtractor.GetLinksFromDocument(response, link);
                else
                    _visitedResources[link.Target] = statusCode;
            }
            finally
            {
                _visitedResources[link.Target] = statusCode;
            }

        link.StatusCode = statusCode;
        crawlResult.IncrementLinksChecked();

        if (link.StatusCode != HttpStatusCode.OK) crawlResult.AddResource(link);

        return links;
    }
}