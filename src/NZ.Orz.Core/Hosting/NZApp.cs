using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NZ.Orz.Config;
using NZ.Orz.Config.Abstractions;
using NZ.Orz.Config.Validators;
using NZ.Orz.Connections;
using NZ.Orz.Hosting;
using NZ.Orz.Metrics;
using NZ.Orz.ReverseProxy.L4;
using NZ.Orz.ReverseProxy.LoadBalancing;
using NZ.Orz.Servers;
using NZ.Orz.ServiceDiscovery;
using NZ.Orz.Sockets;
using NZ.Orz.Sockets.Client;
using System.Diagnostics.Metrics;

namespace NZ.Orz;

public static partial class NZApp
{
    public static IOrzApp CreateBuilder(string[] args = null)
    {
        var builder = new OrzApp(Host.CreateApplicationBuilder(args));
        builder.ApplicationBuilder.UseOrzDefaults();
        builder.ApplicationBuilder.Services.AddSingleton<IHostedService, HostedService>();
        return builder;
    }

    internal static HostApplicationBuilder UseOrzDefaults(this HostApplicationBuilder builder)
    {
        var services = builder.Services;
        services.TryAddSingleton<IMeterFactory, DummyMeterFactory>();
        services.TryAddSingleton<IServer, OrzServer>();
        services.TryAddSingleton<OrzTrace>();
        services.TryAddSingleton<OrzMetrics>();
        services.AddSingleton<IConnectionListenerFactory, SocketTransportFactory>();
        services.AddSingleton<IConnectionListenerFactory, UdpTransportFactory>();
        services.AddSingleton<IConnectionFactory, SocketConnectionFactory>();
        services.AddSingleton<IRouteContractorValidator, RouteContractorValidator>();
        services.AddSingleton<IEndPointConvertor, CommonEndPointConvertor>();
        services.AddSingleton<IL4Router, L4Router>();
        services.AddSingleton<IOrderMiddleware, L4ProxyMiddleware>();
        services.AddSingleton<LoadBalancingPolicy>();
        services.AddSingleton<IClusterConfigValidator, ClusterConfigValidator>();
        services.AddSingleton<IDestinationResolver, DnsDestinationResolver>();

        return builder;
    }
}