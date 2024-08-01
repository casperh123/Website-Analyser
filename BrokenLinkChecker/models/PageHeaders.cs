namespace BrokenLinkChecker.models;

public record PageHeaders
{
    public string CacheControl { get; set; } = "";
    public string CacheStatus { get; set; } = "";
    public string ContentEncoding { get; set; } = "";
    public string LastModified { get; set; } = "";
    public string Server { get; set; } = "";
}