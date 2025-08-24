using System.Net;
using WebsiteAnalyzer.Core.Enums;
using HttpVersion = WebsiteAnalyzer.Core.Enums.HttpVersion;

namespace WebsiteAnalyzer.Core.Domain.Website;

public class GeneralStats
{
    public Guid Id { get; init; }
    public Entities.Website.Website Website { get; set; }
    public WebsiteStatus Status { get; init; }
    public HttpVersion HttpVersion { get; init; }
    public bool EncryptedConnection { get; init; }
    public string ServerIp { get; init; }
    public string ContentEncoding { get; init; }
    public string Server { get; init; }
    public string ServerTiming { get; init; }

    public GeneralStats() { }
    
    public GeneralStats(
        Entities.Website.Website website,
        WebsiteStatus status,
        HttpVersion httpVersion,
        bool encryptedConnection,
        string serverIp,
        string contentEncoding,
        string server,
        string serverTiming
    )
    {
        Id = Guid.NewGuid();
        Website = website;
        Status = status;
        HttpVersion = httpVersion;
        EncryptedConnection = encryptedConnection;
        ServerIp = serverIp;
        ContentEncoding = contentEncoding;
        Server = server;
        ServerTiming = serverTiming;
    }
}