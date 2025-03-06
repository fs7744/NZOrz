using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NZ.Orz.Config.Customize;

public static class NZAppCustomizeExtensions
{
    public static HostApplicationBuilder ConfigureRouteContractor<Contractor>(this HostApplicationBuilder builder) where Contractor : class, IRouteContractor
    {
        builder.Services.AddSingleton<IRouteContractor, Contractor>();
        return builder;
    }

    public static HostApplicationBuilder ConfigureRoute(this HostApplicationBuilder builder, Action<RouteConfigBuilder> action)
    {
        var b = new RouteConfigBuilder
        {
            Services = builder.Services
        };
        action(b);
        var r = new CustomizeRouteConfigContractor(b.ServerOptions, i => b.EndPoints.SelectMany(e =>
        {
            e.ServiceProvider = i;
            return e.Build();
        }).ToArray(), b.SocketTransportOptions);
        builder.Services.AddSingleton<IRouteContractor>(i =>
        {
            r.ServiceProvider = i;
            return r;
        });

        return builder;
    }
}