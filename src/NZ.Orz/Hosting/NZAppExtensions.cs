using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NZ.Orz.Config;
using NZ.Orz.Config.Customize;

namespace NZ.Orz;

public static class NZAppExtensions
{
    public static HostApplicationBuilder ConfigureRouteContractor<Contractor>(this HostApplicationBuilder builder) where Contractor : class, IRouteContractor
    {
        builder.Services.AddSingleton<IRouteContractor, Contractor>();
        return builder;
    }

    public static HostApplicationBuilder ConfigureRoute(this HostApplicationBuilder builder, Action<RouteConfigBuilder> action)
    {
        builder.UseOrzDefaults();

        var b = new RouteConfigBuilder
        {
            Services = builder.Services
        };
        action(b);
        builder.Services.AddTransient<IRouteContractor>(i =>
        {
            return new MemoryRouteConfigContractor(b.ServerOptions, b.EndPoints.Select(e =>
            {
                e.ServiceProvider = i;
                return e.Build();
            }).ToArray());
        });

        return builder;
    }
}