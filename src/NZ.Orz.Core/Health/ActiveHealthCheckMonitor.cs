using NZ.Orz.Config;
using System.Collections.Frozen;

namespace NZ.Orz.Health;

public class ActiveHealthCheckMonitor : IActiveHealthCheckMonitor, IDisposable
{
    private readonly FrozenDictionary<string, IActiveHealthChecker> checkers;
    private readonly IHealthUpdater healthUpdater;

    public ActiveHealthCheckMonitor(TimeProvider timeProvider, IEnumerable<IActiveHealthChecker> checkers, IHealthUpdater healthUpdater)
    {
        Scheduler = new EntityActionScheduler<WeakReference<ClusterConfig>>(ProbeCluster, autoStart: false, runOnce: false, timeProvider);
        this.checkers = checkers.ToFrozenDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
        this.healthUpdater = healthUpdater;
    }

    private async Task ProbeCluster(WeakReference<ClusterConfig> reference)
    {
        if (!reference.TryGetTarget(out var cluster) || cluster.HealthCheck is null || cluster.HealthCheck.Active is null)
        {
            Scheduler.UnscheduleEntity(reference);
            return;
        }

        var config = cluster.HealthCheck.Active;
        if (!checkers.TryGetValue(config.Policy, out var checker))
        {
            //todo log
            Scheduler.UnscheduleEntity(reference);
            return;
        }

        try
        {
            var cts = new CancellationTokenSource(config.Timeout);
            var all = cluster.DestinationStates.ToArray();
            await Task.WhenAll(all.Select(i => checker.CheckAsync(config, i, cts.Token)).ToArray());
            healthUpdater.UpdateAvailableDestinations(cluster);
        }
        catch (Exception ex)
        {
            //todo log
        }
    }

    internal EntityActionScheduler<WeakReference<ClusterConfig>> Scheduler { get; }

    public Task CheckHealthAsync(IEnumerable<ClusterConfig> clusters)
    {
        return Task.Run(async () =>
        {
            try
            {
                var probeClusterTasks = new List<Task>();
                foreach (var cluster in clusters)
                {
                    if (cluster.HealthCheck?.Active != null)
                    {
                        var r = new WeakReference<ClusterConfig>(cluster);
                        Scheduler.ScheduleEntity(r, cluster.HealthCheck.Active.Interval);
                        probeClusterTasks.Add(ProbeCluster(r));
                    }
                }

                await Task.WhenAll(probeClusterTasks);
            }
            catch (Exception ex)
            {
                //todo Log.ExplicitActiveCheckOfAllClustersHealthFailed(_logger, ex);
            }

            Scheduler.Start();
        });
    }

    public void Dispose()
    {
        Scheduler.Dispose();
    }
}