using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using NZ.Orz.Sockets;
using System.Net.Sockets;

namespace NZ.Orz.Config.Configuration;

public class ConfigurationRouteContractor : IRouteContractor, IDisposable
{
    public static string Section = "ReverseProxy";
    private readonly Lock lockObj = new();
    private readonly IConfiguration configuration;
    private IDisposable? subscription;
    private ServerOptions serverOptions;
    private SocketTransportOptions socketTransportOptions;

    public ConfigurationRouteContractor(IConfiguration configuration)
    {
        this.configuration = configuration.GetRequiredSection(Section);
    }

    public void Dispose()
    {
        subscription?.Dispose();
    }

    public IEnumerable<ListenOptions> GetListenOptions()
    {
        throw new NotImplementedException();
    }

    public IProxyConfig GetProxyConfig()
    {
        throw new NotImplementedException();
    }

    public IChangeToken? GetReloadToken()
    {
        throw new NotImplementedException();
    }

    public ServerOptions GetServerOptions()
    {
        return serverOptions;
    }

    public SocketTransportOptions? GetSocketTransportOptions()
    {
        return socketTransportOptions;
    }

    public Task LoadAsync(CancellationToken cancellationToken)
    {
        subscription = ChangeToken.OnChange(configuration.GetReloadToken, UpdateSnapshot);
        UpdateAllSnapshot();
        return Task.CompletedTask;
    }

    private void UpdateAllSnapshot()
    {
        serverOptions = CreateServerOptions(configuration.GetSection(nameof(ServerOptions)));
        socketTransportOptions = CreateSocketTransportOptions(configuration.GetSection(nameof(SocketTransportOptions)));
        UpdateSnapshot();
    }

    private SocketTransportOptions CreateSocketTransportOptions(IConfigurationSection section)
    {
        var s = new SocketTransportOptions();
        if (section.Exists())
        {
            s.ConnectionTimeout = section.ReadTimeSpan(nameof(SocketTransportOptions.ConnectionTimeout)).GetValueOrDefault(s.ConnectionTimeout);
            s.UdpMaxSize = section.ReadInt32(nameof(SocketTransportOptions.UdpMaxSize)).GetValueOrDefault(s.UdpMaxSize);
            s.NoDelay = section.ReadBool(nameof(SocketTransportOptions.NoDelay)).GetValueOrDefault(s.NoDelay);
            s.Backlog = section.ReadInt32(nameof(SocketTransportOptions.Backlog)).GetValueOrDefault(s.Backlog);
            s.FinOnError = section.ReadBool(nameof(SocketTransportOptions.FinOnError)).GetValueOrDefault(s.FinOnError);
            s.IOQueueCount = section.ReadInt32(nameof(SocketTransportOptions.IOQueueCount)).GetValueOrDefault(s.IOQueueCount);
            s.WaitForDataBeforeAllocatingBuffer = section.ReadBool(nameof(SocketTransportOptions.WaitForDataBeforeAllocatingBuffer)).GetValueOrDefault(s.WaitForDataBeforeAllocatingBuffer);
            s.MaxReadBufferSize = section.ReadInt64(nameof(SocketTransportOptions.MaxReadBufferSize), s.MaxReadBufferSize);
            s.MaxWriteBufferSize = section.ReadInt64(nameof(SocketTransportOptions.MaxWriteBufferSize), s.MaxWriteBufferSize);
            s.UnsafePreferInlineScheduling = section.ReadBool(nameof(SocketTransportOptions.UnsafePreferInlineScheduling)).GetValueOrDefault(s.UnsafePreferInlineScheduling);
        }
        return s;
    }

    private ServerOptions CreateServerOptions(IConfigurationSection section)
    {
        var s = new ServerOptions();
        if (section.Exists())
        {
            s.DnsRefreshPeriod = section.ReadTimeSpan(nameof(ServerOptions.DnsRefreshPeriod));
            s.DnsAddressFamily = section.ReadEnum<AddressFamily>(nameof(ServerOptions.DnsAddressFamily));
            s.DefaultProxyTimeout = section.ReadTimeSpan(nameof(ServerOptions.DefaultProxyTimeout)).GetValueOrDefault(TimeSpan.FromSeconds(60));
            CreateLimits(s.Limits, section.GetSection(nameof(ServerOptions.Limits)));
        }
        return s;
    }

    private void CreateLimits(ServerLimits limits, IConfigurationSection section)
    {
        if (!section.Exists()) return;
        limits.MaxConcurrentConnections = section.ReadInt64(nameof(ServerLimits.MaxConcurrentConnections));
        limits.MaxConcurrentUpgradedConnections = section.ReadInt64(nameof(ServerLimits.MaxConcurrentUpgradedConnections));
    }

    private void UpdateSnapshot()
    {
        lock (lockObj)
        {
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }
}