using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using NZ.Orz.Health;
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
    private IList<ListenOptions> listenOptions;
    private ProxyConfigSnapshot proxyConfig;
    private readonly CancellationTokenSource cts;
    private readonly IServiceProvider serviceProvider;

    public ConfigurationRouteContractor(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        this.configuration = configuration.GetRequiredSection(Section);
        this.serviceProvider = serviceProvider;
    }

    public void Dispose()
    {
        subscription?.Dispose();
    }

    public IEnumerable<ListenOptions> GetListenOptions()
    {
        return listenOptions;
    }

    public IProxyConfig GetProxyConfig()
    {
        return proxyConfig;
    }

    public IChangeToken? GetReloadToken()
    {
        //todo
        return null;
    }

    public ServerOptions GetServerOptions()
    {
        return serverOptions;
    }

    public SocketTransportOptions? GetSocketTransportOptions()
    {
        return socketTransportOptions;
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
            var c = new ProxyConfigSnapshot();

            c.Routes = configuration.GetSection(nameof(ProxyConfigSnapshot.Routes)).GetChildren().Select(CreateRoute).ToList();
            c.Clusters = configuration.GetSection(nameof(ProxyConfigSnapshot.Clusters)).GetChildren().Select(CreateCluster).ToList();
            proxyConfig = c;
        }
    }

    private ClusterConfig CreateCluster(IConfigurationSection section)
    {
        return new ClusterConfig()
        {
            ClusterId = section.Key,
            LoadBalancingPolicy = section[nameof(ClusterConfig.LoadBalancingPolicy)],
            HealthCheck = CreateHealthCheck(section.GetSection(nameof(ClusterConfig.HealthCheck))),
            Destinations = section.GetSection(nameof(ClusterConfig.Destinations)).GetChildren().Select(CreateDestination).ToList()
        };
    }

    private DestinationConfig CreateDestination(IConfigurationSection section)
    {
        if (!section.Exists()) return null;
        return new DestinationConfig()
        {
            Address = section[nameof(DestinationConfig.Address)],
            Host = section[nameof(DestinationConfig.Host)],
        };
    }

    private HealthCheckConfig CreateHealthCheck(IConfigurationSection section)
    {
        if (!section.Exists()) return null;
        return new HealthCheckConfig()
        {
            Passive = CreatePassiveHealthCheckConfig(section.GetSection(nameof(HealthCheckConfig.Passive))),
            Active = CreateActiveHealthCheckConfig(section.GetSection(nameof(HealthCheckConfig.Active)))
        };
    }

    private PassiveHealthCheckConfig CreatePassiveHealthCheckConfig(IConfigurationSection section)
    {
        if (!section.Exists()) return null;
        var s = new PassiveHealthCheckConfig();
        s.DetectionWindowSize = section.ReadTimeSpan(nameof(PassiveHealthCheckConfig.DetectionWindowSize)).GetValueOrDefault(s.DetectionWindowSize);
        s.MinimalTotalCountThreshold = section.ReadInt32(nameof(PassiveHealthCheckConfig.MinimalTotalCountThreshold)).GetValueOrDefault(s.MinimalTotalCountThreshold);
        s.FailureRateLimit = section.ReadDouble(nameof(PassiveHealthCheckConfig.FailureRateLimit)).GetValueOrDefault(s.FailureRateLimit);
        s.ReactivationPeriod = section.ReadTimeSpan(nameof(PassiveHealthCheckConfig.ReactivationPeriod)).GetValueOrDefault(s.ReactivationPeriod);
        return s;
    }

    private ActiveHealthCheckConfig CreateActiveHealthCheckConfig(IConfigurationSection section)
    {
        if (!section.Exists()) return null;
        var s = new ActiveHealthCheckConfig();
        s.Interval = section.ReadTimeSpan(nameof(ActiveHealthCheckConfig.Interval)).GetValueOrDefault(s.Interval);
        s.Timeout = section.ReadTimeSpan(nameof(ActiveHealthCheckConfig.Timeout)).GetValueOrDefault(s.Timeout);
        s.Policy = section[nameof(ActiveHealthCheckConfig.Policy)] ?? s.Policy;
        s.Path = section[nameof(ActiveHealthCheckConfig.Path)];
        s.Query = section[nameof(ActiveHealthCheckConfig.Query)];
        s.Passes = section.ReadInt32(nameof(ActiveHealthCheckConfig.Passes)).GetValueOrDefault(s.Passes);
        s.Fails = section.ReadInt32(nameof(ActiveHealthCheckConfig.Fails)).GetValueOrDefault(s.Fails);
        return s;
    }

    private RouteConfig CreateRoute(IConfigurationSection section)
    {
        var serverOptions = GetServerOptions();
        return new RouteConfig()
        {
            RouteId = section.Key,
            Order = section.ReadInt32(nameof(RouteConfig.Order)).GetValueOrDefault(),
            ClusterId = section[nameof(RouteConfig.ClusterId)],
            RetryCount = section.ReadInt32(nameof(RouteConfig.RetryCount)).GetValueOrDefault(),
            Timeout = section.ReadTimeSpan(nameof(RouteConfig.Timeout)).GetValueOrDefault(serverOptions.DefaultProxyTimeout),
            Protocols = section.ReadEnum<GatewayProtocols>(nameof(RouteConfig.Protocols)).GetValueOrDefault(GatewayProtocols.TCP),
            Match = CreateRouteMatch(section.GetSection(nameof(RouteConfig.Match)))
        };
    }

    private RouteMatch CreateRouteMatch(IConfigurationSection section)
    {
        if (!section.Exists()) return null;
        return new RouteMatch()
        {
            Hosts = section.GetSection(nameof(RouteMatch.Hosts)).ReadStringArray()
        };
    }

    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        subscription = ChangeToken.OnChange(configuration.GetReloadToken, UpdateSnapshot);
        UpdateAllSnapshot();
        var errors = new List<Exception>();
        listenOptions = await serviceProvider.GetRequiredService<IRouteContractorValidator>().ValidateAndGenerateListenOptionsAsync(proxyConfig, serverOptions, socketTransportOptions, errors, cancellationToken);
        // todo
        if (errors.Any())
        {
            throw new AggregateException(errors);
        }
        _ = serviceProvider.GetRequiredService<IActiveHealthCheckMonitor>().CheckHealthAsync(proxyConfig.Clusters);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }
}