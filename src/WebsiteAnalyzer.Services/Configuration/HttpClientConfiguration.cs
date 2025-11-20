using System.Net;
using System.Security.Authentication;

namespace WebsiteAnalyzer.Services.Configuration;

public static class HttpClientConfiguration
{
    public static IServiceCollection AddDefaultHttpClient(this IServiceCollection services)
    {
        services.AddHttpClient("", client => // Empty string for default
            {
                client.DefaultRequestVersion = HttpVersion.Version30;
                client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
                client.Timeout = TimeSpan.FromSeconds(20);
                client.DefaultRequestHeaders.ConnectionClose = false;

                client.DefaultRequestHeaders.Add("Connection", "Upgrade, HTTP2-Settings");

                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                    "(KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                client.DefaultRequestHeaders.Accept.ParseAdd(
                    "text/html,application/xhtml+xml,application/xml;q=0.9," +
                    "image/webp,image/apng,*/*;q=0.8");

                client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                UseCookies = false,
                SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                {
                    EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
                },
                MaxConnectionsPerServer = 500,
                AutomaticDecompression = DecompressionMethods.All,
                AllowAutoRedirect = true,
                EnableMultipleHttp2Connections = true,
                EnableMultipleHttp3Connections = true
            });

        // Register HttpClient directly
        services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(""));

        return services;
    }
}