using DotNext;
using DotNext.Collections.Generic;
using Microsoft.Extensions.Options;
using NZ.Orz.Connections;
using NZ.Orz.ReverseProxy.L4;
using NZ.Orz.Routing;
using NZ.Orz.Sockets;
using System.Collections.Frozen;
using System.Diagnostics.Metrics;
using System.Linq;
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
    private readonly ConnectionDelegate middleware;

    public int Order => 0;

    public RouteContractorValidator(IEnumerable<IServerOptionsValidator> serverOptionsValidators,
        IEnumerable<ISocketTransportOptionsValidator> socketTransportOptionsValidators,
        IEnumerable<IClusterConfigValidator> clusterConfigValidators,
        IEnumerable<IRouteConfigValidator> routeConfigValidators,
        IEnumerable<IListenOptionsValidator> listenOptionsValidator,
        IEnumerable<IEndPointConvertor> endPointConvertors,
        IEnumerable<IOrderMiddleware> middlewares)
    {
        this.serverOptionsValidators = serverOptionsValidators.OrderByDescending(i => i.Order).ToArray();
        this.socketTransportOptionsValidators = socketTransportOptionsValidators.OrderByDescending(i => i.Order).ToArray();
        this.clusterConfigValidators = clusterConfigValidators.OrderByDescending(i => i.Order).ToArray(); ;
        this.routeConfigValidators = routeConfigValidators.OrderByDescending(i => i.Order).ToArray();
        this.listenOptionsValidator = listenOptionsValidator.OrderByDescending(i => i.Order).ToArray();
        this.endPointConvertors = endPointConvertors.OrderByDescending(i => i.Order).ToArray();
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
        //todo remove error config and log
        var ec = errors.Count;
        foreach (var cluster in config.Clusters.ToList())
        {
            foreach (var validator in clusterConfigValidators)
            {
                ec = errors.Count;
                await validator.ValidateAsync(cluster, errors, cancellationToken);
                if (errors.Count > ec)
                {
                    config.Clusters.Remove(cluster);
                }
            }
        }

        foreach (var route in config.Routes.ToList())
        {
            foreach (var validator in routeConfigValidators)
            {
                ec = errors.Count;
                await validator.ValidateAsync(route, errors, cancellationToken);
                if (errors.Count > ec)
                {
                    config.Routes.Remove(route);
                }
            }
        }
        var r = Generate(config, serverOptions, errors).ToList();
        foreach (var listenOptions in r.ToList())
        {
            foreach (var validator in listenOptionsValidator)
            {
                ec = errors.Count;
                await validator.ValidateAsync(listenOptions, errors, cancellationToken);
                if (errors.Count > ec)
                {
                    r.Remove(listenOptions);
                }
            }
        }
        return r;
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