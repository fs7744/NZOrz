using DotNext;
using Microsoft.Extensions.Options;
using NZ.Orz.Connections;
using NZ.Orz.ReverseProxy.L4;
using NZ.Orz.Routing;
using NZ.Orz.Sockets;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NZ.Orz.Config.Validators;

public class RouteContractorValidator : IRouteContractorValidator
{
    private readonly IEnumerable<IServerOptionsValidator> serverOptionsValidators;
    private readonly IEnumerable<ISocketTransportOptionsValidator> socketTransportOptionsValidators;
    private readonly IEnumerable<IClusterConfigValidator> clusterConfigValidators;
    private readonly IEnumerable<IRouteConfigValidator> routeConfigValidators;
    private readonly IEnumerable<IListenOptionsValidator> listenOptionsValidator;
    private readonly IEnumerable<IEndPointConvertor> endPointConvertors;
    private readonly IL4Router l4;
    private readonly ConnectionDelegate middleware;

    public int Order => 0;

    public RouteContractorValidator(IEnumerable<IServerOptionsValidator> serverOptionsValidators,
        IEnumerable<ISocketTransportOptionsValidator> socketTransportOptionsValidators,
        IEnumerable<IClusterConfigValidator> clusterConfigValidators,
        IEnumerable<IRouteConfigValidator> routeConfigValidators,
        IEnumerable<IListenOptionsValidator> listenOptionsValidator,
        IEnumerable<IEndPointConvertor> endPointConvertors,
        IEnumerable<IOrderMiddleware> middlewares,
        IL4Router l4)
    {
        this.serverOptionsValidators = serverOptionsValidators.OrderByDescending(i => i.Order).ToArray();
        this.socketTransportOptionsValidators = socketTransportOptionsValidators.OrderByDescending(i => i.Order).ToArray();
        this.clusterConfigValidators = clusterConfigValidators.OrderByDescending(i => i.Order).ToArray(); ;
        this.routeConfigValidators = routeConfigValidators.OrderByDescending(i => i.Order).ToArray();
        this.listenOptionsValidator = listenOptionsValidator.OrderByDescending(i => i.Order).ToArray();
        this.endPointConvertors = endPointConvertors.OrderByDescending(i => i.Order).ToArray();
        this.l4 = l4;
        this.middleware = BuildMiddleware(middlewares);
    }

    private ConnectionDelegate BuildMiddleware(IEnumerable<IOrderMiddleware> middlewares)
    {
        ConnectionDelegate app = context =>
        {
            context.Abort();
            return Task.CompletedTask;
        };
        foreach (var component in middlewares.OrderBy(i => i.Order)
            .Select<IOrderMiddleware, Func<ConnectionDelegate, ConnectionDelegate>>(p => (ConnectionDelegate next) => (ConnectionContext c) => p.Invoke(c, next)))
        {
            app = component(app);
        }
        return app;
    }

    public async ValueTask<IList<ListenOptions>> ValidateAndGenerateListenOptionsAsync(IProxyConfig config, ServerOptions serverOptions, SocketTransportOptions options, IList<Exception> errors, CancellationToken cancellationToken)
    {
        foreach (var cluster in config.Clusters)
        {
            foreach (var validator in clusterConfigValidators)
            {
                await validator.ValidateAsync(cluster, errors, cancellationToken);
            }
        }

        foreach (var route in config.Routes)
        {
            foreach (var validator in routeConfigValidators)
            {
                await validator.ValidateAsync(route, errors, cancellationToken);
            }
        }
        var r = Generate(config, serverOptions, errors).ToList();
        foreach (var listenOptions in r)
        {
            foreach (var validator in listenOptionsValidator)
            {
                await validator.ValidateAsync(listenOptions, errors, cancellationToken);
            }
        }
        if (errors.Count == 0)
        {
            if (r != null && r.Count > 0)
            {
                var old = l4.RouteTable;
                l4.RouteTable = BuildL4RouteTable(config, serverOptions);
                if (old != null)
                    await old.DisposeAsync();
            }
        }
        return r;
    }

    private RouteTable<RouteConfig> BuildL4RouteTable(IProxyConfig config, ServerOptions serverOptions)
    {
        var builder = new RouteTableBuilder<RouteConfig>();
        var clusters = config.Clusters.DistinctBy(i => i.ClusterId, StringComparer.OrdinalIgnoreCase).ToDictionary(i => i.ClusterId, StringComparer.OrdinalIgnoreCase);
        foreach (var route in config.Routes.Where(i => i.Protocols.HasFlag(GatewayProtocols.TCP) || i.Protocols.HasFlag(GatewayProtocols.UDP)))
        {
            if (clusters.TryGetValue(route.ClusterId, out var clusterConfig))
            {
                route.ClusterConfig = clusterConfig;
            }
            foreach (var host in route.Match.Hosts)
            {
                if (host.StartsWith("localhost:"))
                {
                    Set(builder, route, $"127.0.0.1:{host.AsSpan(10)}");
                    Set(builder, route, $"[::1]:{host.AsSpan(10)}");
                }
                Set(builder, route, host);
            }
        }
        return builder.Build();

        static void Set(RouteTableBuilder<RouteConfig> builder, RouteConfig? route, string host)
        {
            if (host.StartsWith("*"))
            {
                builder.Add(host[1..].Reverse(), route, RouteType.Prefix, route.Order);
            }
            else
            {
                builder.Add(host.Reverse(), route, RouteType.Exact, route.Order);
            }
        }
    }

    private IEnumerable<ListenOptions> Generate(IProxyConfig config, ServerOptions serverOptions, IList<Exception> errors)
    {
        if (config != null && config.Routes != null)
        {
            foreach (var item in config.Routes)
            {
                if (item.Protocols.HasFlag(GatewayProtocols.TCP) || item.Protocols.HasFlag(GatewayProtocols.UDP))
                {
                    yield return new ListenOptions()
                    {
                        Key = item.RouteId,
                        Protocols = item.Protocols.HasFlag(GatewayProtocols.TCP) ? GatewayProtocols.TCP : GatewayProtocols.UDP,
                        EndPoints = item.Match.Hosts.SelectMany(i => ConvertEndPoint(i, errors)).Where(i => i != null).ToArray(),
                        ConnectionDelegate = middleware
                    };
                }
            }
        }

        //todo : http ListenOptions
    }

    private IEnumerable<EndPoint> ConvertEndPoint(string address, IList<Exception> errors)
    {
        foreach (var item in endPointConvertors)
        {
            if (item.TryConvert(address, out var endPoint))
                return endPoint;
        }
        errors.Add(new ArgumentException($"'{address}' can not convert to EndPoint."));
        return null;
    }

    public async Task ValidateSystemConfigAsync(ServerOptions serverOptions, SocketTransportOptions socketTransportOptions, CancellationToken cancellationToken)
    {
        if (socketTransportOptions != null)
        {
            foreach (var validator in socketTransportOptionsValidators)
            {
                await validator.ValidateAsync(socketTransportOptions, cancellationToken);
            }
        }
        foreach (var validator in serverOptionsValidators)
        {
            await validator.ValidateAsync(serverOptions, cancellationToken);
        }
    }
}