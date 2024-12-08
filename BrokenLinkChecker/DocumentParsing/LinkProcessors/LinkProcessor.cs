using AngleSharp.Html.Parser;
using BrokenLinkChecker.DocumentParsing.ModularLinkExtraction;
using BrokenLinkChecker.models.Links;

namespace BrokenLinkChecker.DocumentParsing.LinkProcessors;

public class LinkProcessor : ILinkProcessor<Link>
{
    private readonly HttpClient _httpClient;
    private readonly AbstractLinkExtractor<Link> _linkExtractor;

    public LinkProcessor(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _linkExtractor = new LinkExtractor(new HtmlParser());
    }

    public async Task<IEnumerable<Link>> ProcessLinkAsync(Link link)
    {
        IEnumerable<Link> links = [];
        
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(
                link.Target,
                HttpCompletionOption.ResponseHeadersRead
            )
            .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                links = await _linkExtractor.GetLinksFromDocument(response, link).ConfigureAwait(false);
            }
        }
        catch (HttpRequestException e)
        {
            links = [];
        }

        return links;
    }
}