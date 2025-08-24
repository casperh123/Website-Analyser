namespace WebsiteAnalyzer.Core.Domain.Website;

public class GeneralStats
{
    public bool EncryptedConnection { get; init; }
    public string ServerIp { get; init; }
    public string ContentEncoding { get; init; }
    public string Server { get; init; }
}