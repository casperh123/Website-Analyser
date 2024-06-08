using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Security.Authentication;

namespace BrokenLinkChecker.Utility
{
    public static class HttpClientSingleton
    {
        private static readonly Lazy<HttpClient> LazyHttpClient = new (() =>
        {
            var handler = new HttpClientHandler
            {
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                MaxConnectionsPerServer = 50, // Adjust based on your application's needs
            };

            var client = new HttpClient(handler)
            {
                DefaultRequestVersion = HttpVersion.Version30,
                Timeout = TimeSpan.FromSeconds(15), // Set an appropriate timeout
            };

            client.DefaultRequestHeaders.ConnectionClose = false;
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
            client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");

            // ServicePointManager settings
            ServicePointManager.DnsRefreshTimeout = (int)TimeSpan.FromMinutes(10).TotalMilliseconds; // Increase DNS cache duration
            ServicePointManager.DefaultConnectionLimit = 65000;
            ServicePointManager.ReusePort = true; // Enable port reuse
            ServicePointManager.MaxServicePointIdleTime = (int)TimeSpan.FromMinutes(1).TotalMilliseconds; // Set idle connection timeout
            ServicePointManager.Expect100Continue = false; // Disable 100-Continue behavior
            ServicePointManager.UseNagleAlgorithm = false; // Disable Nagle algorithm

            // ThreadPool settings
            ThreadPool.SetMinThreads(100, 100);

            // Advanced socket settings
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            return client;
        });

        public static HttpClient Instance => LazyHttpClient.Value;
    }
}
