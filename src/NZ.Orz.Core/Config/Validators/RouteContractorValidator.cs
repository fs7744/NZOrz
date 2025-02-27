using NZ.Orz.Config.Abstractions;
using NZ.Orz.Sockets;
using System.Net;

namespace NZ.Orz.Config.Validators;

public class RouteContractorValidator : IRouteContractorValidator
{
    private readonly IEnumerable<IServerOptionsValidator> serverOptionsValidators;
    private readonly IEnumerable<ISocketTransportOptionsValidator> socketTransportOptionsValidators;
    private readonly IEnumerable<IClusterConfigValidator> clusterConfigValidators;
    private readonly IEnumerable<IRouteConfigValidator> routeConfigValidators;
    private readonly IEnumerable<IListenOptionsValidator> listenOptionsValidator;
    private readonly IEnumerable<IEndPointConvertor> endPointConvertors;

    public RouteContractorValidator(IEnumerable<IServerOptionsValidator> serverOptionsValidators,
        IEnumerable<ISocketTransportOptionsValidator> socketTransportOptionsValidators,
        IEnumerable<IClusterConfigValidator> clusterConfigValidators,
        IEnumerable<IRouteConfigValidator> routeConfigValidators,
        IEnumerable<IListenOptionsValidator> listenOptionsValidator,
        IEnumerable<IEndPointConvertor> endPointConvertors)
    {
        this.serverOptionsValidators = serverOptionsValidators;
        this.socketTransportOptionsValidators = socketTransportOptionsValidators;
        this.clusterConfigValidators = clusterConfigValidators;
        this.routeConfigValidators = routeConfigValidators;
        this.listenOptionsValidator = listenOptionsValidator;
        this.endPointConvertors = endPointConvertors;
    }

    public async ValueTask<IList<ListenOptions>> ValidateAndGenerateListenOptionsAsync(IProxyConfig config, ServerOptions serverOptions, SocketTransportOptions options, IList<Exception> errors)
    {
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
        var r = Generate(config, serverOptions, errors).ToList();
        foreach (var listenOptions in r)
        {
            foreach (var validator in listenOptionsValidator)
            {
                await validator.ValidateAsync(listenOptions, errors);
            }
        }
        return r;
    }

    private IEnumerable<ListenOptions> Generate(IProxyConfig config, ServerOptions serverOptions, IList<Exception> errors)
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
                        EndPoints = item.Match.Hosts.Select(i => ConvertEndPoint(i, errors)).Where(i => i != null).ToArray()
                    };
                }
            }
        }

        //todo : http ListenOptions
    }

    private EndPoint ConvertEndPoint(string address, IList<Exception> errors)
    {
        foreach (var item in endPointConvertors)
        {
            if (item.TryConvert(address, out var endPoint))
                return endPoint;
        }
        errors.Add(new ArgumentException($"'{address}' can not convert to EndPoint."));
        return null;
    }
}