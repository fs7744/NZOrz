using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NZ.Orz.Hosting;
using NZ.Orz.Metrics;
using NZ.Orz.Servers;
using System.Diagnostics.Metrics;

namespace NZ.Orz;

public static partial class NZApp
{
    public static HostApplicationBuilder CreateBuilder(string[] args = null)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddSingleton<IHostedService, HostedService>();
        return builder;
    }

    public static HostApplicationBuilder UseOrzDefaults(this HostApplicationBuilder builder)
    {
        builder.Services.TryAddSingleton<IMeterFactory, DummyMeterFactory>();
        builder.Services.TryAddSingleton<IServer, OrzServer>();
        builder.Services.TryAddSingleton<OrzTrace>();
        builder.Services.TryAddSingleton<OrzMetrics>();
        return builder;
    }
}