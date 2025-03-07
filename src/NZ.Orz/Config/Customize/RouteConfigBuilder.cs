using Microsoft.Extensions.DependencyInjection;
using NZ.Orz.Sockets;

namespace NZ.Orz.Config.Customize;

public class RouteConfigBuilder
{
    public ServerOptions ServerOptions { get; } = new ServerOptions();

    private SocketTransportOptions _SocketTransportOptions;

    public SocketTransportOptions SocketTransportOptions

    {
        get
        {
            if (_SocketTransportOptions is null)
                _SocketTransportOptions = new SocketTransportOptions();
            return _SocketTransportOptions;
        }
    }

    public IServiceCollection Services { get; internal set; }
    internal List<ListenOptionsBuilder> EndPoints { get; private set; } = new List<ListenOptionsBuilder>();

    public void AddEndPoint(string key, Action<ListenOptionsBuilder> action)
    {
        var builder = new ListenOptionsBuilder(key);
        builder.Services = Services;
        EndPoints.Add(builder);
        action(builder);
    }
}