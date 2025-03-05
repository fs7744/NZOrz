using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using NZ.Orz.Health;
using NZ.Orz.Sockets;

namespace NZ.Orz.Config.Memory;

public sealed class MemoryRouteContractor : IRouteContractor
{
    private readonly ProxyConfigSnapshot proxyConfig;
    private readonly ServerOptions serverOptions;
    private readonly SocketTransportOptions socketTransportOptions;
    private IList<ListenOptions> listenOptions;

    public MemoryRouteContractor(MemoryReverseProxyConfigBuilder builder)
    {
        serverOptions = builder.ServerOptions;
        proxyConfig = new ProxyConfigSnapshot()
        {
            Clusters = builder.Clusters.Select(i => i.Build()).ToList(),
            Routes = builder.Routes.Select(i => i.Build(serverOptions)).ToList(),
        };
        socketTransportOptions = builder.SocketTransportOptions;
    }

    public IServiceProvider ServiceProvider { get; internal set; }

    public IEnumerable<ListenOptions> GetListenOptions()
    {
        return listenOptions;
    }

    public IProxyConfig GetProxyConfig()
    {
        return proxyConfig;
    }

    public IChangeToken? GetReloadToken()
    {
        return null;
    }

    public ServerOptions GetServerOptions()
    {
        return serverOptions;
    }

    public SocketTransportOptions? GetSocketTransportOptions()
    {
        return socketTransportOptions;
    }

    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        var errors = new List<Exception>();
        listenOptions = await ServiceProvider.GetRequiredService<IRouteContractorValidator>().ValidateAndGenerateListenOptionsAsync(proxyConfig, serverOptions, socketTransportOptions, errors, cancellationToken);
        if (errors.Any())
        {
            throw new AggregateException(errors);
        }
        _ = ServiceProvider.GetRequiredService<IActiveHealthCheckMonitor>().CheckHealthAsync(proxyConfig.Clusters);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}