using System.Net;
using System.Security.Authentication;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace WebsiteAnalyzer.Web.Configuration;

public static class ServerConfiguration
{
    public static WebApplicationBuilder ConfigureServer(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            // HTTP endpoint configuration
            serverOptions.Listen(IPAddress.Any, 8080);
            
            // HTTPS endpoint configuration
            serverOptions.Listen(IPAddress.Any, 8081, listenOptions =>
            {
                listenOptions.UseHttps();
                listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
            });

            serverOptions.ConfigureHttpsDefaults(httpsOptions =>
            {
                httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
            });
        });

        return builder;
    }
}