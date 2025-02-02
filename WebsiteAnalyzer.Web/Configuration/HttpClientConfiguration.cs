using System.Net;
using System.Security.Authentication;

namespace WebsiteAnalyzer.Web.Configuration
{
    public static class HttpClientConfiguration
    {
        public static IServiceCollection AddHttpClients(this IServiceCollection services)
        {
            HttpClient client = new HttpClient(CreateHttpMessageHandler());
            
            ConfigureHttpClient(client);

            services.AddSingleton<HttpClient>(_ => client);

            return services;
        }

        private static void ConfigureHttpClient(HttpClient client)
        {
            client.DefaultRequestVersion = HttpVersion.Version30;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

            client.Timeout = TimeSpan.FromSeconds(10);

            client.DefaultRequestHeaders.ConnectionClose = false;

            // Set a realistic user agent to avoid being blocked by websites
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                "(KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

            // Configure accepted content types
            client.DefaultRequestHeaders.Accept.ParseAdd(
                "text/html,application/xhtml+xml,application/xml;q=0.9," +
                "image/webp,image/apng,*/*;q=0.8");

            // Enable compression for better performance
            client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");

            // Prevent caching to ensure fresh content
            client.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue
            {
                NoCache = true,
                NoStore = true,
                MustRevalidate = true,
                Private = true
            };
        }

        private static HttpMessageHandler CreateHttpMessageHandler()
        {
            return new HttpClientHandler
            {
                UseCookies = false,
                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                MaxConnectionsPerServer = 50,
                MaxAutomaticRedirections = 10,
                AutomaticDecompression = DecompressionMethods.All,
                AllowAutoRedirect = true,
                CheckCertificateRevocationList = true
            };
        }
    }
}