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
        builder.Services.AddTransient<IRouteContractor>(i => new MemoryRouteConfigContractor(action, i));
        return builder;
    }
}