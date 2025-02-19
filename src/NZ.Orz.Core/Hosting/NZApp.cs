using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NZ.Orz.Connections;
using NZ.Orz.Hosting;
using NZ.Orz.Metrics;
using NZ.Orz.Servers;
using NZ.Orz.Sockets;
using NZ.Orz.Sockets.Client;
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
        builder.Services.AddSingleton<IConnectionListenerFactory, SocketTransportFactory>();
        builder.Services.AddSingleton<IConnectionListenerFactory, UdpTransportFactory>();
        builder.Services.AddSingleton<IConnectionFactory, SocketConnectionFactory>();

        return builder;
    }
}