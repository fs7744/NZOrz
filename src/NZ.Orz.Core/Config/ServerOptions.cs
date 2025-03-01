using System.Net.Sockets;

namespace NZ.Orz.Config;

public class ServerOptions
{
    public ServerLimits Limits { get; } = new ServerLimits();

    public TimeSpan DefaultProxyTimeout { get; init; } = TimeSpan.FromSeconds(60);

    public TimeSpan? DnsRefreshPeriod { get; set; } = TimeSpan.FromSeconds(5);
    public AddressFamily? DnsAddressFamily { get; set; }
}