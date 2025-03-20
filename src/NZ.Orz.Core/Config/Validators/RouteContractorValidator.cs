using DotNext;
using DotNext.Collections.Generic;
using NZ.Orz.Connections;
using NZ.Orz.Metrics;
using NZ.Orz.Sockets;
using System.Net;

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
    private readonly MultiplexedConnectionDelegate multiplexedConnectionMiddleware;
    private readonly ICertificateLoader certificateLoader;
    private readonly OrzLogger logger;

    public int Order => 0;

    public RouteContractorValidator(ICertificateLoader certificateLoader, IEnumerable<IServerOptionsValidator> serverOptionsValidators,
        IEnumerable<ISocketTransportOptionsValidator> socketTransportOptionsValidators,
        IEnumerable<IClusterConfigValidator> clusterConfigValidators,
        IEnumerable<IRouteConfigValidator> routeConfigValidators,
        IEnumerable<IListenOptionsValidator> listenOptionsValidator,
        IEnumerable<IEndPointConvertor> endPointConvertors,
        IEnumerable<IOrderMiddleware> middlewares,
        IEnumerable<IOrderMultiplexedConnectionMiddleware> multiplexedConnectionMiddlewares, OrzLogger logger)
    {
        this.serverOptionsValidators = serverOptionsValidators.OrderByDescending(i => i.Order).ToArray();
        this.socketTransportOptionsValidators = socketTransportOptionsValidators.OrderByDescending(i => i.Order).ToArray();
        this.clusterConfigValidators = clusterConfigValidators.OrderByDescending(i => i.Order).ToArray(); ;
        this.routeConfigValidators = routeConfigValidators.OrderByDescending(i => i.Order).ToArray();
        this.listenOptionsValidator = listenOptionsValidator.OrderByDescending(i => i.Order).ToArray();
        this.endPointConvertors = endPointConvertors.OrderByDescending(i => i.Order).ToArray();
        this.middleware = BuildMiddleware(middlewares);
        multiplexedConnectionMiddleware = BuildMiddleware(multiplexedConnectionMiddlewares);
        this.certificateLoader = certificateLoader;
        this.logger = logger;
    }

    private MultiplexedConnectionDelegate? BuildMiddleware(IEnumerable<IOrderMultiplexedConnectionMiddleware> middlewares)
    {
        MultiplexedConnectionDelegate app = context =>
        {
            context.Abort();
            return Task.CompletedTask;
        };
        foreach (var component in middlewares.OrderBy(i => i.Order)
            .Select<IMultiplexedConnectionMiddleware, Func<MultiplexedConnectionDelegate, MultiplexedConnectionDelegate>>(p => (MultiplexedConnectionDelegate next) => (MultiplexedConnectionContext c) => p.Invoke(c, next)))
        {
            app = component(app);
        }
        return app;
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

    public async ValueTask<List<ListenOptions>> ValidateAndGenerateListenOptionsAsync(ProxyConfigSnapshot config, ServerOptions serverOptions, SocketTransportOptions options, IList<Exception> errors, CancellationToken cancellationToken)
    {
        var ec = errors.Count;
        var clusters = config.Clusters.ToDictionary(i => i.Key, i => i.Value, StringComparer.OrdinalIgnoreCase);
        foreach (var cluster in config.Clusters.Values)
        {
            foreach (var validator in clusterConfigValidators)
            {
                ec = errors.Count;
                await validator.ValidateAsync(cluster, errors, cancellationToken);
                if (errors.Count > ec)
                {
                    clusters.Remove(cluster.ClusterId);
                    logger.RemoveErrorCluster(cluster.ClusterId);
                }
            }
        }
        config.Clusters = clusters;

        var routes = config.Routes.ToList();
        foreach (var route in config.Routes.ToList())
        {
            foreach (var validator in routeConfigValidators)
            {
                ec = errors.Count;
                await validator.ValidateAsync(route, errors, cancellationToken);
                if (route.Ssl is not null && errors.Count == ec)
                {
                    try
                    {
                        route.Ssl.Init(certificateLoader, route);
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex);
                    }
                }
                if (errors.Count > ec)
                {
                    routes.Remove(route);
                    logger.RemoveErrorRoute(route.RouteId);
                }
            }
        }
        config.Routes = routes;

        var r = Generate(config, serverOptions, errors).ToList();
        foreach (var item in r.GroupBy(j => j.Protocols == GatewayProtocols.UDP || j.Protocols == GatewayProtocols.HTTP3).SelectMany(j =>
        {
            return j.GroupBy(i => i.EndPoint.ToString(), StringComparer.OrdinalIgnoreCase).Where(i => i.Count() > 1);
        }))
        {
            errors.Add(new ArgumentException($"There is some conflict with EndPoint: '{item.Key}' and config: {string.Join(",", item.Select(i => i.Key).Distinct(StringComparer.OrdinalIgnoreCase))}."));
            foreach (var i in item)
            {
                r.Remove(i);
            }
        }
        foreach (var listenOptions in r.ToList())
        {
            foreach (var validator in listenOptionsValidator)
            {
                ec = errors.Count;
                await validator.ValidateAsync(listenOptions, errors, cancellationToken);
                if (errors.Count > ec)
                {
                    r.Remove(listenOptions);
                    logger.RemoveErrorListenOptions(listenOptions);
                }
            }
        }
        return r;
    }

    private IEnumerable<ListenOptions> Generate(IProxyConfig config, ServerOptions serverOptions, IList<Exception> errors)
    {
        if (config == null) yield break;
        if (config.Listen != null)
        {
            foreach (var item in config.Listen.Values)
            {
                var p = item.Protocols;
                if (p.HasFlag(GatewayProtocols.SNI))
                {
                    var es = item.Address.SelectMany(i => ConvertEndPoint(i, errors)).Where(i => i != null).ToArray();
                    foreach (var e in es)
                    {
                        yield return new ListenOptions()
                        {
                            Key = $"listen_{item.ListenId}",
                            Protocols = GatewayProtocols.SNI,
                            EndPoint = e,
                            ConnectionDelegate = middleware
                        };
                    }
                }
                else
                {
                    var h1 = p.HasFlag(GatewayProtocols.HTTP1);
                    var h2 = p.HasFlag(GatewayProtocols.HTTP2);
                    var h3 = p.HasFlag(GatewayProtocols.HTTP3);
                    if (h3)
                    {
                        var es = item.Address.SelectMany(i => ConvertEndPoint(i, errors)).Where(i => i != null).ToArray();
                        foreach (var e in es)
                        {
                            yield return new ListenOptions()
                            {
                                Key = $"listen_{item.ListenId}",
                                Protocols = GatewayProtocols.HTTP3,
                                EndPoint = e,
                                MultiplexedConnectionDelegate = multiplexedConnectionMiddleware
                            };
                        }
                    }
                    else if (h1 || h2)
                    {
                        var es = item.Address.SelectMany(i => ConvertEndPoint(i, errors)).Where(i => i != null).ToArray();
                        var ps = h1 && h2 ? GatewayProtocols.HTTP1 | GatewayProtocols.HTTP2 : (h1 ? GatewayProtocols.HTTP1 : GatewayProtocols.HTTP2);
                        foreach (var e in es)
                        {
                            yield return new ListenOptions()
                            {
                                Key = $"listen_{item.ListenId}",
                                Protocols = ps,
                                EndPoint = e,
                                ConnectionDelegate = middleware
                            };
                        }
                    }
                    else
                    {
                        errors.Add(new ArgumentException($"Common listen {item.ListenId} not support TCP or UDP, it can't match upstream."));
                    }
                }
            }
        }
        if (config.Routes != null)
        {
            foreach (var item in config.Routes.Where(i => i.Protocols.HasFlag(GatewayProtocols.TCP) || i.Protocols.HasFlag(GatewayProtocols.UDP)))
            {
                var es = item.Match.Hosts.SelectMany(i => ConvertEndPoint(i, errors)).Where(i => i != null).ToArray();
                if (item.Protocols.HasFlag(GatewayProtocols.TCP))
                {
                    foreach (var e in es)
                    {
                        yield return new ListenOptions()
                        {
                            Key = $"route_{item.RouteId}",
                            Protocols = GatewayProtocols.TCP,
                            EndPoint = e,
                            ConnectionDelegate = middleware
                        };
                    }
                }

                if (item.Protocols.HasFlag(GatewayProtocols.UDP))
                {
                    foreach (var e in es)
                    {
                        yield return new ListenOptions()
                        {
                            Key = $"route_{item.RouteId}",
                            Protocols = GatewayProtocols.UDP,
                            EndPoint = e,
                            ConnectionDelegate = middleware
                        };
                    }
                }
            }
        }
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