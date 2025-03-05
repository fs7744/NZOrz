using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NZ.Orz.Config;
using NZ.Orz.Config.Validators;
using NZ.Orz.Connections;
using NZ.Orz.Health;
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
        return Host.CreateApplicationBuilder(args).UseReverseProxy();
    }

    internal static HostApplicationBuilder UseOrzDefaults(this HostApplicationBuilder builder)
    {
        var services = builder.Services;
        services.AddSingleton<IHostedService, HostedService>();
        services.TryAddSingleton(TimeProvider.System);
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

        services.AddSingleton<ILoadBalancingPolicy, RandomLoadBalancingPolicy>();
        services.AddSingleton<ILoadBalancingPolicy, RoundRobinLoadBalancingPolicy>();
        services.AddSingleton<ILoadBalancingPolicy, LeastRequestsLoadBalancingPolicy>();
        services.AddSingleton<ILoadBalancingPolicy, PowerOfTwoChoicesLoadBalancingPolicy>();

        services.AddSingleton<IHealthReporter, PassiveHealthReporter>();
        services.AddSingleton<IHealthUpdater, HealthyAndUnknownDestinationsUpdater>();

        return builder;
    }
}