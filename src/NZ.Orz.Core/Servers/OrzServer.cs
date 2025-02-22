using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Features;
using NZ.Orz.Infrastructure;
using NZ.Orz.Metrics;
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
    private readonly OrzMetrics metrics;
    private readonly OrzTrace trace;
    private readonly TransportManager _transportManager;
    public IFeatureCollection Features { get; }
    private ServiceContext ServiceContext { get; }

    public OrzServer(IRouteContractor contractor,
        IEnumerable<IConnectionListenerFactory> transportFactories,
        IEnumerable<IMultiplexedConnectionListenerFactory> multiplexedFactories,
        OrzMetrics metrics,
        OrzTrace trace)
    {
        this.contractor = contractor;
        var serverOptions = contractor.GetServerOptions();
        this.metrics = metrics;
        this.trace = trace;
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
            ServerOptions = contractor.GetServerOptions(),
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

        foreach (var endPoint in options.EndPoints)
        {
            var protocols = options.Protocols;
            if (protocols.HasFlag(GatewayProtocols.TCP) || protocols.HasFlag(GatewayProtocols.UDP))
            {
                await _transportManager.BindAsync(endPoint, options.Protocols, options.ConnectionDelegate, options, cancellationToken);
            }

            //if (options.Protocols.HasFlag(GatewayProtocols.UDP))
            //{
            //    await _transportManager.BindAsync(endPoint, options.Protocols, options.MultiplexedConnectionDelegate, options, cancellationToken);
            //}
        }
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
            foreach (var listenOptions in contractor.GetListenOptions())
            {
                await OnBind(listenOptions, cancellationToken).ConfigureAwait(false);
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
            // todo
            //var (endpointsToStop, endpointsToStart) = contractor.Reload();

            //trace.LogDebug("Config reload token fired. Checking for changes...");

            //if (endpointsToStop.Count > 0)
            //{
            //    var urlsToStop = endpointsToStop.Select(lo => lo.EndpointConfig!.Url);
            //    if (trace.IsEnabled(LogLevel.Information))
            //    {
            //        trace.LogInformation("Config changed. Stopping the following endpoints: '{endpoints}'", string.Join("', '", urlsToStop));
            //    }

            //    // 5 is the default value for WebHost's "shutdownTimeoutSeconds", so use that.
            //    using var cts = CancellationTokenSource.CreateLinkedTokenSource(_stopCts.Token);
            //    cts.CancelAfter(TimeSpan.FromSeconds(5));

            //    // TODO: It would be nice to start binding to new endpoints immediately and reconfigured endpoints as soon
            //    // as the unbinding finished for the given endpoint rather than wait for all transports to unbind first.
            //    var configsToStop = new List<EndpointConfig>(endpointsToStop.Count);
            //    foreach (var lo in endpointsToStop)
            //    {
            //        configsToStop.Add(lo.EndpointConfig!);
            //    }
            //    await _transportManager.StopEndpointsAsync(configsToStop, cts.Token).ConfigureAwait(false);

            //    foreach (var listenOption in endpointsToStop)
            //    {
            //        Options.OptionsInUse.Remove(listenOption);
            //        _serverAddresses.InternalCollection.Remove(listenOption.GetDisplayName());
            //    }
            //}

            //if (endpointsToStart.Count > 0)
            //{
            //    var urlsToStart = endpointsToStart.Select(lo => lo.EndpointConfig!.Url);
            //    if (trace.IsEnabled(LogLevel.Information))
            //    {
            //        trace.LogInformation("Config changed. Starting the following endpoints: '{endpoints}'", string.Join("', '", urlsToStart));
            //    }

            //    foreach (var listenOption in endpointsToStart)
            //    {
            //        try
            //        {
            //            await listenOption.BindAsync(AddressBindContext!, _stopCts.Token).ConfigureAwait(false);
            //        }
            //        catch (Exception ex)
            //        {
            //            if (trace.IsEnabled(LogLevel.Critical))
            //            {
            //                trace.LogCritical(0, ex, "Unable to bind to '{url}' on config reload.", listenOption.EndpointConfig!.Url);
            //            }
            //        }
            //    }
            //}
        }
        catch (Exception ex)
        {
            trace.LogCritical(0, ex, "Unable to reload configuration.");
        }
        finally
        {
            _configChangedRegistration = reloadToken?.RegisterChangeCallback(TriggerRebind, this);
            _bindSemaphore.Release();
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