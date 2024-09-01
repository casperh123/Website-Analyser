using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using BrokenLinkChecker.crawler;
using BrokenLinkChecker.DocumentParsing.Browsing;
using BrokenLinkChecker.models;
using BrokenLinkChecker.utility;

namespace BrokenLinkChecker.DocumentParsing.Linkextraction
{
    public class LinkExtractor
    {
        private HtmlParserPool _htmlParserPool;

        public LinkExtractor(CrawlerConfig crawlerConfig)
        {
            HtmlParserOptions htmlParsingOptions = new HtmlParserOptions { IsKeepingSourceReferences = true };
            _htmlParserPool = new HtmlParserPool(htmlParsingOptions, crawlerConfig.ConcurrentRequests);
        }

        public async Task<List<Link>> GetLinksFromResponseAsync(HttpResponseMessage response, Link url)
        {
            if (!IsHtmlContent(response))
            {
                return new List<Link>();
            }

            await using Stream document = await response.Content.ReadAsStreamAsync();

            return await ExtractLinksFromDocumentAsync(document, url);
        }

        private bool IsHtmlContent(HttpResponseMessage response)
        {
            string? contentType = response.Content.Headers.ContentType?.MediaType;
            return contentType != null && contentType.Equals("text/html", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<List<Link>> ExtractLinksFromDocumentAsync(Stream document, Link checkingUrl)
        {
            List<Link> links = new List<Link>();
            IDocument doc;
            Uri thisUrl = new Uri(checkingUrl.Target);

            using (PooledHtmlParser pooledHtmlParser = await _htmlParserPool.GetParserAsync())
            {
                doc = await pooledHtmlParser.Parser.ParseDocumentAsync(document);
            }
            
            foreach (var stylesheet in doc.StyleSheets)
            {
                string href = stylesheet.Href;
                if (!string.IsNullOrEmpty(href))
                {
                    Link newLink = GenerateLinkNode(stylesheet.OwnerNode, checkingUrl.Target, "href", ResourceType.Stylesheet);
                    links.Add(newLink);
                }
            }

            // Extract links from scripts
            foreach (var script in doc.Scripts)
            {
                string? src = script.Source;
                if (!string.IsNullOrEmpty(src))
                {
                    Link newLink = GenerateLinkNode(script, checkingUrl.Target, "src", ResourceType.Script);
                    links.Add(newLink);
                }
            }

            // Extract links from images
            foreach (IHtmlImageElement image in doc.Images)
            {
                string? src = image.Source;
                if (!string.IsNullOrEmpty(src))
                {
                    Link newLink = GenerateLinkNode(image, checkingUrl.Target, "src", ResourceType.Image);
                    links.Add(newLink);
                }
            }

            // Extract anchor links
            foreach (IElement link in doc.Links)
            {
                string? href = link.GetAttribute("href");
                if (!string.IsNullOrEmpty(href))
                {
                    Link newLink = GenerateLinkNode(link, checkingUrl.Target, "href", ResourceType.Page);
                    links.Add(newLink);
                }
            }

            // Optionally, filter out links that are external to the host
            links = links.Where(link => Uri.TryCreate(link.Target, UriKind.Absolute, out Uri uri) && uri.Host == thisUrl.Host).ToList();

            return links;
        }

        private Link GenerateLinkNode(IElement element, string target, string attribute, ResourceType resourceType = ResourceType.Resource)
        {
            string href = element.GetAttribute(attribute) ?? string.Empty;
            string resolvedUrl = Utilities.GetUrl(target, href);
            string text = element.TextContent;
            int line = element.SourceReference?.Position.Line ?? -1;

            return new Link(target, resolvedUrl, text, line, resourceType);
        }
    }
}
