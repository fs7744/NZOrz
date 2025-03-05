using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NZ.Orz;

public static class HostingExtensions
{
    public static IOrzApp UseReverseProxy(this HostApplicationBuilder builder)
    {
        builder.UseOrzDefaults();
        IOrzApp app = new OrzApp(builder);
        return app;
    }

    public static IOrzApp ConfigServices(this IOrzApp app, Action<IServiceCollection> action)
    {
        action(app.Services);
        return app;
    }
}