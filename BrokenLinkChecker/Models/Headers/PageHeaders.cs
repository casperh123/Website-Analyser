using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.;
using BrokenLinkChecker.models;

namespace BrokenLinkChecker.Models.Headers;

public record PageHeaders
{
    public string ContentEncoding { get; set; } = "";
    public string LastModified { get; set; } = "";
    public string Server { get; set; } = "";
    public Cache Cache { get; set; } = new Cache();

    public PageHeaders(HttpResponseHeaders headers)
    {
        ContentEncoding = headers.ContentEncoding ?? "";
        LastModified = headers.LastModified?.ToString() ?? "";
        Server = headers?.ToString() ?? "";
        Cache = new Cache(headers);
    }
}