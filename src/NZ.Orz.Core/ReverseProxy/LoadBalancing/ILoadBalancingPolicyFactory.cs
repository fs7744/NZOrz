using NZ.Orz.Config;
using NZ.Orz.Connections;

namespace NZ.Orz.ReverseProxy.LoadBalancing;

public interface ILoadBalancingPolicyFactory
{
    DestinationState? PickDestination(ConnectionContext context, RouteConfig route);
}