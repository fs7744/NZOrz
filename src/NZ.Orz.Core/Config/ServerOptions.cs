﻿using System.Net.Sockets;

namespace NZ.Orz.Config;

public class ServerOptions
{
    public ServerLimits Limits { get; } = new ServerLimits();

    public TimeSpan DefaultProxyTimeout { get; set; } = TimeSpan.FromSeconds(60);

    public TimeSpan? DnsRefreshPeriod { get; set; } = TimeSpan.FromMinutes(5);
    public AddressFamily? DnsAddressFamily { get; set; }
}