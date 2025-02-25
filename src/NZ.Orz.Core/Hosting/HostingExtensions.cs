using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NZ.Orz.Config;

namespace NZ.Orz;

public static class HostingExtensions
{
    public static IOrzApp UseReverseProxy(this HostApplicationBuilder builder)
    {
        builder.UseOrzDefaults();
        IOrzApp app = new OrzApp(builder);
        return app;
    }
}