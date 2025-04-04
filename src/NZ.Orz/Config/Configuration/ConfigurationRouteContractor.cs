﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using NZ.Orz.Metrics;
using NZ.Orz.Routing;
using NZ.Orz.Sockets;
using System.Collections.Frozen;
using System.Net.Sockets;

namespace NZ.Orz.Config.Configuration;

public class ConfigurationRouteContractor : IRouteContractor, IDisposable
{
    public static string Section = "ReverseProxy";
    private readonly SemaphoreSlim configChangedSemaphore = new SemaphoreSlim(initialCount: 1);
    private readonly IConfiguration configuration;
    private IDisposable? subscription;
    private ServerOptions serverOptions;
    private SocketTransportOptions socketTransportOptions;
    private List<ListenOptions> listenOptions;
    private IProxyConfig proxyConfig;
    private CancellationTokenSource cts;
    private IChangeToken changeToken;
    private ChangedProxyConfig changedProxyConfig;
    private readonly IServiceProvider serviceProvider;
    private readonly OrzLogger logger;

    public ConfigurationRouteContractor(IConfiguration configuration, IServiceProvider serviceProvider, OrzLogger logger)
    {
        this.configuration = configuration.GetRequiredSection(Section);
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    public void Dispose()
    {
        subscription?.Dispose();
    }

    public List<ListenOptions> GetListenOptions()
    {
        return listenOptions;
    }

    public IProxyConfig GetProxyConfig()
    {
        return proxyConfig;
    }

    public IChangeToken? GetReloadToken()
    {
        return changeToken;
    }

    public ServerOptions GetServerOptions()
    {
        return serverOptions;
    }

    public SocketTransportOptions? GetSocketTransportOptions()
    {
        return socketTransportOptions;
    }

    private async Task LoadSystemConfigAsync(CancellationToken cancellationToken)
    {
        serverOptions = CreateServerOptions(configuration.GetSection(nameof(ServerOptions)));
        socketTransportOptions = CreateSocketTransportOptions(configuration.GetSection(nameof(SocketTransportOptions)));
        await serviceProvider.GetRequiredService<IRouteContractorValidator>().ValidateSystemConfigAsync(serverOptions, socketTransportOptions, cancellationToken);
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
            s.UdpPoolSize = section.ReadInt32(nameof(SocketTransportOptions.UdpPoolSize)).GetValueOrDefault(s.UdpPoolSize);
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
            s.DefaultProxyTimeout = section.ReadTimeSpan(nameof(ServerOptions.DefaultProxyTimeout)).GetValueOrDefault(s.DefaultProxyTimeout);
            s.ShutdownTimeout = section.ReadTimeSpan(nameof(ServerOptions.ShutdownTimeout)).GetValueOrDefault(s.ShutdownTimeout);
            s.RouteCahceSize = section.ReadInt32(nameof(ServerOptions.RouteCahceSize)).GetValueOrDefault(s.RouteCahceSize);
            s.RouteComparison = section.ReadEnum<StringComparison>(nameof(ServerOptions.RouteComparison)).GetValueOrDefault(s.RouteComparison);
            s.L4RouteType = section.ReadEnum<RouteTableType>(nameof(ServerOptions.L4RouteType)).GetValueOrDefault(s.L4RouteType);
            s.DisableHttp1LineFeedTerminators = section.ReadBool(nameof(ServerOptions.DisableHttp1LineFeedTerminators)).GetValueOrDefault(s.DisableHttp1LineFeedTerminators);
            CreateLimits(s.Limits, section.GetSection(nameof(ServerOptions.Limits)));
        }
        return s;
    }

    private void CreateLimits(ServerLimits limits, IConfigurationSection section)
    {
        if (!section.Exists()) return;
        limits.MaxConcurrentConnections = section.ReadInt64(nameof(ServerLimits.MaxConcurrentConnections));
        limits.MaxConcurrentUpgradedConnections = section.ReadInt64(nameof(ServerLimits.MaxConcurrentUpgradedConnections));
        limits.KeepAliveTimeout = section.ReadTimeSpan(nameof(ServerLimits.KeepAliveTimeout)).GetValueOrDefault(limits.KeepAliveTimeout);
        limits.RequestHeadersTimeout = section.ReadTimeSpan(nameof(ServerLimits.RequestHeadersTimeout)).GetValueOrDefault(limits.RequestHeadersTimeout);
        limits.MaxRequestLineSize = section.ReadInt32(nameof(ServerLimits.MaxRequestLineSize)).GetValueOrDefault(limits.MaxRequestLineSize);
        limits.MaxRequestHeadersTotalSize = section.ReadInt32(nameof(ServerLimits.MaxRequestHeadersTotalSize)).GetValueOrDefault(limits.MaxRequestHeadersTotalSize);
        limits.MaxRequestHeaderCount = section.ReadInt32(nameof(ServerLimits.MaxRequestHeaderCount)).GetValueOrDefault(limits.MaxRequestHeaderCount);
        if (configuration.GetSection(nameof(ServerLimits.MaxResponseBufferSize)).Exists())
        {
            limits.MaxResponseBufferSize = section.ReadInt64(nameof(ServerLimits.MaxResponseBufferSize));
        }
        if (configuration.GetSection(nameof(ServerLimits.MaxRequestBufferSize)).Exists())
        {
            limits.MaxRequestBufferSize = section.ReadInt64(nameof(ServerLimits.MaxRequestBufferSize));
        }
        if (configuration.GetSection(nameof(ServerLimits.MaxRequestBodySize)).Exists())
        {
            limits.MaxRequestBodySize = section.ReadInt64(nameof(ServerLimits.MaxRequestBodySize));
        }
        var s = configuration.GetSection(nameof(ServerLimits.MinRequestBodyDataRate));
        if (s.Exists())
        {
            limits.MinRequestBodyDataRate = CreateMinDataRate(s);
        }
        s = configuration.GetSection(nameof(ServerLimits.MinResponseDataRate));
        if (s.Exists())
        {
            limits.MinResponseDataRate = CreateMinDataRate(s);
        }
    }

    private MinDataRate? CreateMinDataRate(IConfigurationSection section)
    {
        if (!section.Exists()) return null;
        return new MinDataRate(section.ReadDouble(nameof(MinDataRate.BytesPerSecond)).GetValueOrDefault(240),
            section.ReadTimeSpan(nameof(MinDataRate.GracePeriod)).GetValueOrDefault(TimeSpan.FromSeconds(5)));
    }

    private async Task UpdateSnapshotAsync(CancellationToken cancellationToken)
    {
        await configChangedSemaphore.WaitAsync();
        ProxyConfigSnapshot c;
        try
        {
            c = new ProxyConfigSnapshot();
            c.Routes = configuration.GetSection(nameof(ProxyConfigSnapshot.Routes)).GetChildren().Select(CreateRoute).ToList();
            c.Clusters = configuration.GetSection(nameof(ProxyConfigSnapshot.Clusters)).GetChildren().Select(CreateCluster).ToFrozenDictionary(i => i.ClusterId, StringComparer.OrdinalIgnoreCase);
            c.Listen = configuration.GetSection(nameof(ProxyConfigSnapshot.Listen)).GetChildren().Select(CreateListen).ToFrozenDictionary(i => i.ListenId, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            logger.ConfigError(ex.Message);
            if (proxyConfig is null)
            {
                throw;
            }

            return;
        }
        finally
        {
            configChangedSemaphore.Release();
        }

        try
        {
            var oldToken = cts;
            cts = new CancellationTokenSource();
            changeToken = new CancellationChangeToken(cts.Token);
            await OnConfigChanged(oldToken, proxyConfig as ProxyConfigSnapshot, c, listenOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.UnexpectedException("Config changed", ex);
        }
    }

    private ListenConfig CreateListen(IConfigurationSection section)
    {
        return new ListenConfig()
        {
            ListenId = section.Key,
            Protocols = section.ReadGatewayProtocols(nameof(ListenConfig.Protocols)).GetValueOrDefault(GatewayProtocols.SNI),
            Address = section.GetSection(nameof(ListenConfig.Address)).ReadStringArray()
        };
    }

    private async Task OnConfigChanged(CancellationTokenSource oldToken, ProxyConfigSnapshot oldConf, ProxyConfigSnapshot newConf, List<ListenOptions> listenOptions, CancellationToken cancellationToken)
    {
        var errors = new List<Exception>();
        var newListenOptions = await serviceProvider.GetRequiredService<IRouteContractorValidator>().ValidateAndGenerateListenOptionsAsync(newConf, serverOptions, socketTransportOptions, errors, cancellationToken);
        foreach (var e in errors)
        {
            logger.ConfigError(e.Message);
        }
        changedProxyConfig = MergeConfig(newConf, oldConf, listenOptions, newListenOptions);
        if (changedProxyConfig != null)
        {
            proxyConfig = changedProxyConfig.ProxyConfig;
        }
        else
        {
            proxyConfig = newConf;
        }
        this.listenOptions = newListenOptions;
        oldToken?.Cancel(throwOnFirstException: false);
    }

    private ChangedProxyConfig MergeConfig(ProxyConfigSnapshot newConf, ProxyConfigSnapshot oldConf, List<ListenOptions> oldListenOptions, List<ListenOptions> newListenOptions)
    {
        if (oldConf is null && oldListenOptions is null) return null;
        var changedProxyConfig = new ChangedProxyConfig();
        if (oldListenOptions != null)
        {
            var set = oldListenOptions.ToDictionary(i => i.GetHashCode());
            var start = newListenOptions.ToList();
            foreach (var listenOption in newListenOptions)
            {
                var key = listenOption.GetHashCode();
                if (set.TryGetValue(key, out var v) && v.Equals(listenOption))
                {
                    set.Remove(key);
                    start.Remove(listenOption);
                }
            }

            changedProxyConfig.EndpointsToStop = set.Values.ToList();
            changedProxyConfig.EndpointsToStart = start;
        }
        else
        {
            changedProxyConfig.EndpointsToStart = newListenOptions;
        }

        if (oldConf != null)
        {
            var dict = newConf.Clusters.ToDictionary(StringComparer.OrdinalIgnoreCase);
            var newClusters = newConf.Clusters.Values.ToList();
            foreach (var item in oldConf.Clusters.Values)
            {
                if (dict.TryGetValue(item.ClusterId, out var c) && c.Equals(item))
                {
                    newClusters.Remove(c);
                    dict[item.ClusterId] = item;
                }
            }
            newConf.Clusters = dict;
            changedProxyConfig.NewClusters = newClusters;
            if (newConf.Routes.Count != oldConf.Routes.Count)
            {
                changedProxyConfig.RouteChanged = true;
            }
            else
            {
                var old = oldConf.Routes.ToFrozenDictionary(i => i.RouteId, StringComparer.OrdinalIgnoreCase);
                changedProxyConfig.RouteChanged = newConf.Routes.Any(i => !old.TryGetValue(i.RouteId, out var c) || !i.Equals(c));
            }
        }
        else
        {
            changedProxyConfig.RouteChanged = true;
        }
        changedProxyConfig.ProxyConfig = newConf;

        return changedProxyConfig;
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
        if (!section.Exists() || !section.ReadBool("Enable").GetValueOrDefault()) return null;
        var s = new PassiveHealthCheckConfig();
        s.DetectionWindowSize = section.ReadTimeSpan(nameof(PassiveHealthCheckConfig.DetectionWindowSize)).GetValueOrDefault(s.DetectionWindowSize);
        s.MinimalTotalCountThreshold = section.ReadInt32(nameof(PassiveHealthCheckConfig.MinimalTotalCountThreshold)).GetValueOrDefault(s.MinimalTotalCountThreshold);
        s.FailureRateLimit = section.ReadDouble(nameof(PassiveHealthCheckConfig.FailureRateLimit)).GetValueOrDefault(s.FailureRateLimit);
        s.ReactivationPeriod = section.ReadTimeSpan(nameof(PassiveHealthCheckConfig.ReactivationPeriod)).GetValueOrDefault(s.ReactivationPeriod);
        return s;
    }

    private ActiveHealthCheckConfig CreateActiveHealthCheckConfig(IConfigurationSection section)
    {
        if (!section.Exists() || !section.ReadBool("Enable").GetValueOrDefault()) return null;
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
            UdpResponses = section.ReadInt32(nameof(RouteConfig.UdpResponses)).GetValueOrDefault(),
            Timeout = section.ReadTimeSpan(nameof(RouteConfig.Timeout)).GetValueOrDefault(serverOptions.DefaultProxyTimeout),
            Protocols = section.ReadGatewayProtocols(nameof(RouteConfig.Protocols)).GetValueOrDefault(GatewayProtocols.HTTP1 | GatewayProtocols.HTTP2),
            Match = CreateRouteMatch(section.GetSection(nameof(RouteConfig.Match))),
            Ssl = CreateSslConfig(section.GetSection(nameof(RouteConfig.Ssl)))
        };
    }

    private SslConfig CreateSslConfig(IConfigurationSection section)
    {
        if (!section.Exists()) return null;
        var s = new SslConfig()
        {
            Path = section[nameof(SslConfig.Path)],
            KeyPath = section[nameof(SslConfig.KeyPath)],
            Password = section[nameof(SslConfig.Password)],
            Subject = section[nameof(SslConfig.Subject)],
            Store = section[nameof(SslConfig.Store)],
            Location = section[nameof(SslConfig.Location)],
            AllowInvalid = section.ReadBool(nameof(SslConfig.AllowInvalid)),
        };
        s.SupportSslProtocols = section.ReadSslProtocols(nameof(SslConfig.SupportSslProtocols)).GetValueOrDefault(s.SupportSslProtocols);
        s.Passthrough = section.ReadBool(nameof(SslConfig.Passthrough)).GetValueOrDefault(s.Passthrough);
        s.HandshakeTimeout = section.ReadTimeSpan(nameof(SslConfig.HandshakeTimeout)).GetValueOrDefault(s.HandshakeTimeout);
        s.ClientCertificateRequired = section.ReadBool(nameof(SslConfig.ClientCertificateRequired)).GetValueOrDefault(s.ClientCertificateRequired);
        s.CheckCertificateRevocation = section.ReadBool(nameof(SslConfig.CheckCertificateRevocation)).GetValueOrDefault(s.CheckCertificateRevocation);
        return s;
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
        subscription = ChangeToken.OnChange(configuration.GetReloadToken, UpdateSnapshotSync);
        await LoadSystemConfigAsync(cancellationToken);
        await UpdateSnapshotAsync(cancellationToken);
    }

    private void UpdateSnapshotSync()
    {
        UpdateSnapshotAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }

    public Task<ChangedProxyConfig> ReloadAsync()
    {
        var old = changedProxyConfig;
        changedProxyConfig = null;
        return Task.FromResult(old);
    }
}