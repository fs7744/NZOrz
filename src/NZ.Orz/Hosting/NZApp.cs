using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NZ.Orz.Config;
using NZ.Orz.Hosting;

namespace NZ.Orz;

public static class NZApp
{
    public static HostApplicationBuilder CreateBuilder(string[] args = null)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddSingleton<IHostedService, HostedService>();
        return builder;
    }

    public static HostApplicationBuilder ConfigureRouteContractor<Contractor>(this HostApplicationBuilder builder) where Contractor : class, IRouteContractor
    {
        builder.Services.AddSingleton<IRouteContractor, Contractor>();
        return builder;
    }

    public static HostApplicationBuilder ConfigureMemoryRouteConfig(this HostApplicationBuilder builder, RouteConfig config)
    {
        builder.Services.AddSingleton<IRouteContractor>(new MemoryRouteConfigContractor(config));
        return builder;
    }
}