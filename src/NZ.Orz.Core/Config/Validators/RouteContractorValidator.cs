using NZ.Orz.Sockets;

namespace NZ.Orz.Config.Validators;

public class RouteContractorValidator : IRouteContractorValidator
{
    private readonly IEnumerable<IServerOptionsValidator> serverOptionsValidators;
    private readonly IEnumerable<ISocketTransportOptionsValidator> socketTransportOptionsValidators;
    private readonly IEnumerable<IClusterConfigValidator> clusterConfigValidators;
    private readonly IEnumerable<IRouteConfigValidator> routeConfigValidators;
    private readonly IEnumerable<IListenOptionsValidator> listenOptionsValidator;

    public RouteContractorValidator(IEnumerable<IServerOptionsValidator> serverOptionsValidators,
        IEnumerable<ISocketTransportOptionsValidator> socketTransportOptionsValidators,
        IEnumerable<IClusterConfigValidator> clusterConfigValidators,
        IEnumerable<IRouteConfigValidator> routeConfigValidators,
        IEnumerable<IListenOptionsValidator> listenOptionsValidator)
    {
        this.serverOptionsValidators = serverOptionsValidators;
        this.socketTransportOptionsValidators = socketTransportOptionsValidators;
        this.clusterConfigValidators = clusterConfigValidators;
        this.routeConfigValidators = routeConfigValidators;
        this.listenOptionsValidator = listenOptionsValidator;
    }

    public async ValueTask<IList<ListenOptions>> ValidateAndGenerateListenOptionsAsync(IProxyConfig config, ServerOptions serverOptions, SocketTransportOptions options)
    {
        var errors = new List<Exception>();
        if (options != null)
        {
            foreach (var validator in socketTransportOptionsValidators)
            {
                await validator.ValidateAsync(options, errors);
            }
        }
        foreach (var validator in serverOptionsValidators)
        {
            await validator.ValidateAsync(serverOptions, errors);
        }
        foreach (var cluster in config.Clusters)
        {
            foreach (var validator in clusterConfigValidators)
            {
                await validator.ValidateAsync(cluster, errors);
            }
        }

        foreach (var route in config.Routes)
        {
            foreach (var validator in routeConfigValidators)
            {
                await validator.ValidateAsync(route, errors);
            }
        }
        var r = Generate(config, serverOptions).ToList();
        foreach (var listenOptions in r)
        {
            foreach (var validator in listenOptionsValidator)
            {
                await validator.ValidateAsync(listenOptions, errors);
            }
        }
        return r;
    }

    private IEnumerable<ListenOptions> Generate(IProxyConfig config, ServerOptions serverOptions)
    {
        if (config != null && config.Routes != null)
        {
            foreach (var item in config.Routes)
            {
                if (item.Protocols.HasFlag(GatewayProtocols.TCP) || item.Protocols.HasFlag(GatewayProtocols.UDP))
                {
                    yield return new ListenOptions()
                    {
                        Key = item.RouteId,
                        Protocols = item.Protocols.HasFlag(GatewayProtocols.TCP) ? GatewayProtocols.TCP : GatewayProtocols.UDP,
                        //todo :    EndPoints = item.Match.Hosts
                    };
                }
            }
        }

        //todo : http ListenOptions
    }
}