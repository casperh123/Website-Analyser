using System.Net;

namespace BrokenLinkChecker;

public static class HttpClientSingleton
{
    private static readonly Lazy<HttpClient> lazyHttpClient = new Lazy<HttpClient>(() =>
    {
        var handler = new HttpClientHandler
        {
            UseCookies = false,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
            SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13,
        };
        var client = new HttpClient(handler)
        {
            DefaultRequestVersion = HttpVersion.Version30
        };

        client.DefaultRequestHeaders.ConnectionClose = false;
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
        client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");

        return client;
    });

    public static HttpClient Instance => lazyHttpClient.Value;
}