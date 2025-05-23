﻿using NZ.Orz.Routing;
using System.Net.Sockets;

namespace NZ.Orz.Config;

public class ServerOptions
{
    public ServerLimits Limits { get; } = new ServerLimits();

    public TimeSpan DefaultProxyTimeout { get; set; } = TimeSpan.FromSeconds(300);

    public TimeSpan? DnsRefreshPeriod { get; set; } = TimeSpan.FromMinutes(5);
    public AddressFamily? DnsAddressFamily { get; set; }
    public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(5);

    public int RouteCahceSize { get; set; } = 10240;

    public StringComparison RouteComparison { get; set; } = StringComparison.OrdinalIgnoreCase;
    public RouteTableType L4RouteType { get; set; } = RouteTableType.OnlyFirst;
    public bool DisableHttp1LineFeedTerminators { get; set; }
}