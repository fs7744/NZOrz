using NZ.Orz.Health;
using NZ.Orz.ReverseProxy.LoadBalancing;
using NZ.Orz.ServiceDiscovery;
using System.Collections.Frozen;
using System.Net;
using System.Net.Sockets;

namespace NZ.Orz.Config.Validators;

public class ClusterConfigValidator : IClusterConfigValidator
{
    private readonly IEnumerable<IDestinationResolver> resolvers;
    private readonly FrozenDictionary<string, ILoadBalancingPolicy> policies;
    private readonly IHealthReporter healthReporter;
    private readonly IHealthUpdater healthUpdater;

    public ClusterConfigValidator(IEnumerable<IDestinationResolver> resolvers, IEnumerable<ILoadBalancingPolicy> policies, IHealthReporter healthReporter, IHealthUpdater healthUpdater)
    {
        this.resolvers = resolvers.OrderByDescending(i => i.Order).ToArray();
        this.policies = policies.ToFrozenDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
        this.healthReporter = healthReporter;
        this.healthUpdater = healthUpdater;
    }

    public async ValueTask ValidateAsync(ClusterConfig cluster, IList<Exception> errors, CancellationToken cancellationToken)
    {
        if (policies.TryGetValue(cluster.LoadBalancingPolicy ?? LoadBalancingPolicy.Random, out var policy))
        {
            cluster.LoadBalancingPolicyInstance = policy;
        }
        else
        {
            errors.Add(new NotSupportedException($"Not supported LoadBalancingPolicy : {cluster.LoadBalancingPolicy}"));
            return;
        }

        if (cluster.HealthCheck != null)
        {
            var passive = cluster.HealthCheck.Passive;
            if (passive != null)
            {
                passive.ReactivationPeriod = passive.ReactivationPeriod >= passive.DetectionWindowSize ? passive.ReactivationPeriod : passive.DetectionWindowSize;
                cluster.HealthReporter = healthReporter;
            }
        }

        var destinationStates = new List<DestinationState>();
        List<DestinationConfig> destinationConfigs = new List<DestinationConfig>();
        foreach (var d in cluster.Destinations)
        {
            var address = d.Address;
            if (IPEndPoint.TryParse(address, out var ip))
            {
                destinationStates.Add(new DestinationState() { EndPoint = ip, ClusterConfig = cluster });
            }
            else if (File.Exists(address))
            {
                destinationStates.Add(new DestinationState() { EndPoint = new UnixDomainSocketEndPoint(address), ClusterConfig = cluster });
            }
            else if (address.StartsWith("localhost:", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(address.AsSpan(10), out var port)
                && port >= IPEndPoint.MinPort && port <= IPEndPoint.MaxPort)
            {
                destinationStates.Add(new DestinationState() { EndPoint = new IPEndPoint(IPAddress.Loopback, port), ClusterConfig = cluster });
                destinationStates.Add(new DestinationState() { EndPoint = new IPEndPoint(IPAddress.IPv6Loopback, port), ClusterConfig = cluster });
            }
            else
            {
                destinationConfigs.Add(d);
            }
        }

        if (destinationConfigs.Count > 0)
        {
            List<IDestinationResolverState> states = new List<IDestinationResolverState>();
            if (resolvers.Any())
            {
                foreach (var resolver in resolvers)
                {
                    try
                    {
                        var r = await resolver.ResolveDestinationsAsync(cluster, destinationConfigs, cancellationToken);
                        if (r != null)
                        {
                            states.Add(r);
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add(new InvalidOperationException($"Error resolving destinations for cluster {cluster.ClusterId}", ex));
                    }
                }
            }
            else
            {
                errors.Add(new InvalidOperationException($"No DestinationResolver for cluster {cluster.ClusterId}"));
            }

            if (destinationStates.Count > 0)
            {
                states.Insert(0, new StaticDestinationResolverState(destinationStates));
            }

            cluster.DestinationStates = new UnionDestinationResolverState(states);
        }
        else
        {
            if (destinationStates.Count > 0)
            {
                cluster.DestinationStates = destinationStates;
            }
        }
        healthUpdater.UpdateAvailableDestinations(cluster);
    }
}