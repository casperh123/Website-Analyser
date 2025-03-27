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
            serverOptions.Listen(IPAddress.Any, 8080);
        });

        return builder;
    }
}