using Microsoft.Extensions.Primitives;
using NZ.Orz.Sockets;

namespace NZ.Orz.Config;

public interface IRouteContractor
{
    IEnumerable<ListenOptions> GetListenOptions();

    IChangeToken? GetReloadToken();

    ServerOptions GetServerOptions();

    SocketTransportOptions? GetSocketTransportOptions();

    IProxyConfig GetProxyConfig();

    Task LoadAsync(CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);

    //todo : move to validator
    static IEnumerable<ListenOptions> Generate(IProxyConfig config, ServerOptions serverOptions)
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