﻿using DotNext;
using NZ.Orz.Buffers;
using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Infrastructure.Tls;
using NZ.Orz.Metrics;
using NZ.Orz.Routing;
using System.IO.Pipelines;
using System.Security.Authentication;

namespace NZ.Orz.ReverseProxy.L4;

public class L4Router : IL4Router
{
    private IRouteTable<RouteConfig> routeTable;
    private IRouteTable<RouteConfig> sniRoute;
    private readonly OrzLogger logger;
    private bool onlyFirst;

    public L4Router(OrzLogger logger)
    {
        this.logger = logger;
    }

    public ValueTask<RouteConfig> MatchAsync(ConnectionContext context)
    {
        if (routeTable is null) return ValueTask.FromResult<RouteConfig>(null);
        return routeTable.MatchAsync(context.LocalEndPointString, context.Protocols, Match);
    }

    private static bool Match(RouteConfig config, GatewayProtocols protocols)
    {
        return config.Protocols.HasFlag(protocols);
    }

    public async Task ReBulidAsync(IProxyConfig proxyConfig, ServerOptions serverOptions)
    {
        var old = routeTable;
        var oldSniRoute = sniRoute;
        (routeTable, sniRoute) = BuildL4RouteTable(proxyConfig, serverOptions);
        if (old != null)
            await old.DisposeAsync();
        if (oldSniRoute != null)
            await oldSniRoute.DisposeAsync();
    }

    private (IRouteTable<RouteConfig> l4, IRouteTable<RouteConfig> sni) BuildL4RouteTable(IProxyConfig config, ServerOptions serverOptions)
    {
        var builder = new RouteTableBuilder<RouteConfig>(serverOptions.RouteComparison, serverOptions.RouteCahceSize);
        var sniRouteBuilder = new RouteTableBuilder<RouteConfig>(serverOptions.RouteComparison, serverOptions.RouteCahceSize);
        var hasL4 = false;
        var hasSni = false;
        foreach (var route in config.Routes.Where(i => i.Protocols.HasFlag(GatewayProtocols.TCP) || i.Protocols.HasFlag(GatewayProtocols.UDP) || i.Protocols.HasFlag(GatewayProtocols.SNI)))
        {
            RouteTableBuilder<RouteConfig> b;
            if (route.Protocols.HasFlag(GatewayProtocols.SNI))
            {
                b = sniRouteBuilder;
                hasSni = true;
            }
            else
            {
                hasL4 = true;
                b = builder;
            };
            foreach (var host in route.Match.Hosts)
            {
                if (host.StartsWith("localhost:"))
                {
                    Set(b, route, $"127.0.0.1:{host.AsSpan(10)}");
                    Set(b, route, $"[::1]:{host.AsSpan(10)}");
                }
                Set(b, route, host);
            }
        }
        onlyFirst = serverOptions.L4RouteType == RouteTableType.OnlyFirst;
        return (hasL4 ? builder.Build(serverOptions.L4RouteType) : null, hasSni ? sniRouteBuilder.Build(serverOptions.L4RouteType) : null);

        static void Set(RouteTableBuilder<RouteConfig> builder, RouteConfig? route, string host)
        {
            if (host.StartsWith('*'))
            {
                builder.Add(host[1..].Reverse(), route, RouteType.Prefix, route.Order);
            }
            else
            {
                builder.Add(host.Reverse(), route, RouteType.Exact, route.Order);
            }
        }
    }

    public async ValueTask<(RouteConfig, ReadResult)> MatchSNIAsync(ConnectionContext context, CancellationToken token)
    {
        if (sniRoute is null) return (null, default);
        var (hello, rr) = await TryGetClientHelloAsync(context, token);
        if (hello.HasValue)
        {
            var h = hello.Value;
            if (onlyFirst)
            {
                var r = await sniRoute.MatchAsync(h.TargetName.Reverse(), context.Protocols, Match);
                if (r is null || !MatchSNI(r, h))
                {
                    logger.NotFoundRouteSni(h.TargetName);
                }
                return (r, rr);
            }
            else
            {
                var r = await sniRoute.MatchAsync(h.TargetName.Reverse(), h, MatchSNI);
                if (r is null)
                {
                    logger.NotFoundRouteSni(h.TargetName);
                }
                return (r, rr);
            }
        }
        else
        {
            logger.NotFoundRouteSni("client hello failed");
            return (null, rr);
        }
    }

    private bool MatchSNI(RouteConfig config, TlsFrameInfo info)
    {
        if (config.Ssl is null) return false;
        var v = config.Ssl.SupportSslProtocols;
        if (v == SslProtocols.None) return true;
        var t = info.SupportedVersions;
        if (v.HasFlag(SslProtocols.Tls13) && t.HasFlag(SslProtocols.Tls13)) return true;
        else if (v.HasFlag(SslProtocols.Tls12) && t.HasFlag(SslProtocols.Tls12)) return true;
        else if (v.HasFlag(SslProtocols.Tls11) && t.HasFlag(SslProtocols.Tls11)) return true;
        else if (v.HasFlag(SslProtocols.Tls) && t.HasFlag(SslProtocols.Tls)) return true;
        else if (v.HasFlag(SslProtocols.Ssl3) && t.HasFlag(SslProtocols.Ssl3)) return true;
        else if (v.HasFlag(SslProtocols.Ssl2) && t.HasFlag(SslProtocols.Ssl2)) return true;
        else if (v.HasFlag(SslProtocols.Default) && t.HasFlag(SslProtocols.Default)) return true;
        else return false;
    }

    private static async ValueTask<(TlsFrameInfo?, ReadResult)> TryGetClientHelloAsync(ConnectionContext context, CancellationToken token)
    {
        var input = context.Transport.Input;
        TlsFrameInfo info = default;
        while (true)
        {
            var f = await input.ReadAsync(token).ConfigureAwait(false);
            if (f.IsCompleted)
            {
                return (null, f);
            }
            var buffer = f.Buffer;
            if (buffer.Length == 0)
            {
                continue;
            }

            var data = buffer.ToSpan();
            if (TlsFrameHelper.TryGetFrameInfo(data, ref info))
            {
                return (info, f);
            }
            else
            {
                input.AdvanceTo(buffer.Start, buffer.End);
                continue;
            }
        }
    }
}