using System.Net;
using System.Security.Authentication;

namespace WebsiteAnalyzer.Web.Configuration
{
    public static class HttpClientConfiguration
    {
        public static IServiceCollection AddHttpClients(this IServiceCollection services)
        {
            services.ConfigureHttpClientDefaults(builder =>
            {
                // Configure the primary message handler
                builder.ConfigurePrimaryHttpMessageHandler(() => 
                {
                    // Use SocketsHttpHandler for better HTTP/3 support
                    return new SocketsHttpHandler
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
                    };
                });

                // Configure the client
                builder.ConfigureHttpClient(client =>
                {
                    // Set HTTP version preferences
                    client.DefaultRequestVersion = HttpVersion.Version30;
                    client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

                    client.Timeout = TimeSpan.FromSeconds(20);
                    client.DefaultRequestHeaders.ConnectionClose = false;

                    // Add diagnostic header for HTTP/2 & HTTP/3
                    client.DefaultRequestHeaders.Add("Connection", "Upgrade, HTTP2-Settings");

                    // Set a realistic user agent
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                        "(KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                    // Configure accepted content types
                    client.DefaultRequestHeaders.Accept.ParseAdd(
                        "text/html,application/xhtml+xml,application/xml;q=0.9," +
                        "image/webp,image/apng,*/*;q=0.8");

                    // Enable compression
                    client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
                });
            });

            return services;
        }
    }
}