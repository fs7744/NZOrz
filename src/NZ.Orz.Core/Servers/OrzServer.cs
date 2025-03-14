using Microsoft.Extensions.Primitives;
using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Features;
using NZ.Orz.Health;
using NZ.Orz.Infrastructure;
using NZ.Orz.Metrics;
using NZ.Orz.ReverseProxy.L4;
using System.IO.Pipelines;

namespace NZ.Orz.Servers;

public class OrzServer : IServer
{
    private bool _hasStarted;
    private int _stopping;
    private IDisposable? _configChangedRegistration;
    private readonly SemaphoreSlim _bindSemaphore = new SemaphoreSlim(initialCount: 1);
    private readonly CancellationTokenSource _stopCts = new CancellationTokenSource();
    private readonly TaskCompletionSource _stoppedTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly IRouteContractor contractor;
    private readonly ServerOptions serverOptions;
    private readonly OrzMetrics metrics;
    private readonly OrzLogger trace;
    private readonly IL4Router l4;
    private readonly IActiveHealthCheckMonitor monitor;
    private readonly TransportManager _transportManager;
    public IFeatureCollection Features { get; }
    private ServiceContext ServiceContext { get; }

    public OrzServer(IRouteContractor contractor,
        IEnumerable<IConnectionListenerFactory> transportFactories,
        IEnumerable<IMultiplexedConnectionListenerFactory> multiplexedFactories,
        OrzMetrics metrics,
        OrzLogger trace,
        IL4Router l4,
        IActiveHealthCheckMonitor monitor)
    {
        this.contractor = contractor;
        this.serverOptions = contractor.GetServerOptions();
        this.metrics = metrics;
        this.trace = trace;
        this.l4 = l4;
        this.monitor = monitor;
        Features = new FeatureCollection();
        var connectionManager = new ConnectionManager(
            trace,
            serverOptions.Limits.MaxConcurrentUpgradedConnections);

        var heartbeat = new Heartbeat(
            [
                connectionManager
            ],
            TimeProvider.System,
            trace,
            Heartbeat.Interval);

        ServiceContext = new ServiceContext
        {
            Log = trace,
            Scheduler = PipeScheduler.ThreadPool,
            TimeProvider = TimeProvider.System,
            ConnectionManager = connectionManager,
            Heartbeat = heartbeat,
            ServerOptions = serverOptions,
            Metrics = metrics
        };
        _transportManager = new TransportManager(Enumerable.Reverse(transportFactories).ToList(), Enumerable.Reverse(multiplexedFactories).ToList(), ServiceContext);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_hasStarted)
            {
                throw new InvalidOperationException("Server already started");
            }
            _hasStarted = true;

            ServiceContext.Heartbeat?.Start();
            var proxyConfig = contractor.GetProxyConfig();
            if (proxyConfig is not null)
            {
                MakeSureConfig(proxyConfig);
                await ReloadRouteAsync(proxyConfig, true);
                _ = monitor.CheckHealthAsync(proxyConfig.Clusters.Values);
            }
            await BindAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    private async Task OnBind(ListenOptions options, CancellationToken cancellationToken)
    {
        // todo support tcp / udp / http 1 2 3

        var protocols = options.Protocols;
        if (protocols.HasFlag(GatewayProtocols.TCP) || protocols.HasFlag(GatewayProtocols.UDP))
        {
            await _transportManager.BindAsync(options.EndPoint, options.Protocols, options.ConnectionDelegate, options, cancellationToken);
        }

        //if (options.Protocols.HasFlag(GatewayProtocols.UDP))
        //{
        //    await _transportManager.BindAsync(endPoint, options.Protocols, options.MultiplexedConnectionDelegate, options, cancellationToken);
        //}
    }

    private async Task BindAsync(CancellationToken cancellationToken)
    {
        await _bindSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (_stopping == 1)
            {
                throw new InvalidOperationException("Server has already been stopped.");
            }

            IChangeToken? reloadToken = contractor.GetReloadToken();

            var toStart = contractor.GetListenOptions();
            trace.StartEndpointsInfo(toStart);
            foreach (var listenOptions in toStart)
            {
                try
                {
                    await OnBind(listenOptions, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    trace.BindListenOptionsError(listenOptions, ex);
                }
            }
            _configChangedRegistration = reloadToken?.RegisterChangeCallback(TriggerRebind, this);
        }
        finally
        {
            _bindSemaphore.Release();
        }
    }

    private static void TriggerRebind(object? state)
    {
        if (state is OrzServer server)
        {
            _ = server.RebindAsync();
        }
    }

    private async Task RebindAsync()
    {
        await _bindSemaphore.WaitAsync();

        IChangeToken? reloadToken = null;
        var trace = ServiceContext.Log;
        try
        {
            if (_stopping == 1)
            {
                return;
            }

            reloadToken = contractor.GetReloadToken();
            var changedProxyConfig = await contractor.ReloadAsync();
            if (changedProxyConfig != null)
            {
                MakeSureConfig(changedProxyConfig.ProxyConfig);
                await ReloadRouteAsync(changedProxyConfig.ProxyConfig, changedProxyConfig.RouteChanged);
                if (changedProxyConfig.NewClusters != null)
                    _ = monitor.CheckHealthAsync(changedProxyConfig.NewClusters);

                var toStop = changedProxyConfig.EndpointsToStop;
                if (toStop != null && toStop.Count > 0)
                {
                    trace.StopEndpointsInfo(toStop);
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(_stopCts.Token);
                    cts.CancelAfter(serverOptions.ShutdownTimeout);
                    await _transportManager.StopEndpointsAsync(toStop, cts.Token).ConfigureAwait(false);
                }

                var toStart = changedProxyConfig.EndpointsToStart;
                if (toStart != null && toStart.Count > 0)
                {
                    trace.StartEndpointsInfo(toStart);
                    foreach (var listenOptions in toStart)
                    {
                        try
                        {
                            await OnBind(listenOptions, _stopCts.Token).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            trace.BindListenOptionsError(listenOptions, ex);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            trace.UnexpectedException("Unable to reload configuration", ex);
        }
        finally
        {
            _configChangedRegistration = reloadToken?.RegisterChangeCallback(TriggerRebind, this);
            _bindSemaphore.Release();
        }
    }

    private async Task ReloadRouteAsync(IProxyConfig proxyConfig, bool changed)
    {
        if (changed)
        {
            await l4.ReBulidAsync(proxyConfig, contractor.GetServerOptions());
        }
    }

    private static void MakeSureConfig(IProxyConfig proxyConfig)
    {
        foreach (var route in proxyConfig.Routes)
        {
            if (proxyConfig.Clusters.TryGetValue(route.ClusterId, out var clusterConfig))
            {
                route.ClusterConfig = clusterConfig;
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _stopping, 1) == 1)
        {
            await _stoppedTcs.Task.ConfigureAwait(false);
            return;
        }

        ServiceContext.Heartbeat?.Dispose();

        _stopCts.Cancel();

        await _bindSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            await _transportManager.StopAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _stoppedTcs.TrySetException(ex);
            throw;
        }
        finally
        {
            _configChangedRegistration?.Dispose();
            _stopCts.Dispose();
            _bindSemaphore.Release();
        }

        _stoppedTcs.TrySetResult();
    }

    public void Dispose()
    {
        StopAsync(new CancellationToken(canceled: true)).GetAwaiter().GetResult();
    }
}