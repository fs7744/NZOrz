using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Features;
using NZ.Orz.Infrastructure;
using NZ.Orz.Metrics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace NZ.Orz.Servers;

public class OrzServer : IServer
{
    private bool _hasStarted;
    private int _stopping;
    private readonly SemaphoreSlim _bindSemaphore = new SemaphoreSlim(initialCount: 1);
    private readonly IRouteContractor contractor;
    private readonly OrzMetrics metrics;
    private readonly OrzTrace trace;
    public IFeatureCollection Features { get; }
    private ServiceContext ServiceContext { get; }

    public OrzServer(IRouteContractor contractor,
        IEnumerable<IConnectionListenerFactory> transportFactories,
        OrzMetrics metrics,
        OrzTrace trace)
    {
        this.contractor = contractor;
        this.metrics = metrics;
        this.trace = trace;
        Features = new FeatureCollection();

        var heartbeat = new Heartbeat(
            new IHeartbeatHandler[] {
                //todo
            },
            TimeProvider.System,
            trace,
            Heartbeat.Interval);

        ServiceContext = new ServiceContext
        {
            Log = trace,
            Scheduler = PipeScheduler.ThreadPool,
            TimeProvider = TimeProvider.System,
            Heartbeat = heartbeat,
            ServerOptions = contractor.GetServerOptions(),
            Metrics = metrics
        };
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

    private async Task BindAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        StopAsync(new CancellationToken(canceled: true)).GetAwaiter().GetResult();
    }
}