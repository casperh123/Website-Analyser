using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace WebsiteAnalyser.Api.Configuration;

public static class ServerConfiguration
{
    public static WebApplicationBuilder ConfigureServer(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.Listen(IPAddress.Any, 8000, options =>
            {
                options.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
            });
        });

        return builder;
    }
}