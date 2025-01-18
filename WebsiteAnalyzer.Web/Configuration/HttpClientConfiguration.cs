using System.Net;
using System.Security.Authentication;

namespace WebsiteAnalyzer.Web.Configuration
{
    public static class HttpClientConfiguration
    {
        public static IServiceCollection AddHttpClients(this IServiceCollection services)
        {
            // Configure the named HttpClient for website analysis
            // Using a named client allows different configurations for different purposes
            services.AddHttpClient("WebsiteAnalyser", client =>
            {
                ConfigureHttpClient(client);
            }).ConfigurePrimaryHttpMessageHandler(() => CreateHttpMessageHandler());

            return services;
        }

        private static void ConfigureHttpClient(HttpClient client)
        {
            // Set HTTP version to HTTP/3 for improved performance
            client.DefaultRequestVersion = HttpVersion.Version30;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

            // Set a reasonable timeout to prevent hanging requests
            client.Timeout = TimeSpan.FromSeconds(20);

            // Keep connections alive for better performance
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

            // Set language preference
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");

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
                // Disable cookie handling as we manage them manually
                UseCookies = false,

                // Configure SSL/TLS protocols for security
                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,

                // Configure connection pooling
                MaxConnectionsPerServer = 50,
                MaxAutomaticRedirections = 10,

                // Enable all compression methods for better performance
                AutomaticDecompression = DecompressionMethods.All,

                // Configure proxy settings if needed
                UseProxy = true,
                UseDefaultCredentials = false,

                // Configure SSL/TLS certificate validation
                // WARNING: In production, you should implement proper certificate validation
                ServerCertificateCustomValidationCallback = 
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,

                // Enable automatic redirection following
                AllowAutoRedirect = true,

                // Configure checking certificate revocation
                CheckCertificateRevocationList = true
            };
        }
    }
}