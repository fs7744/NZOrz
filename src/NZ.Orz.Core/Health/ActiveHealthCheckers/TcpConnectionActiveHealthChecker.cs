using NZ.Orz.Config;
using NZ.Orz.Connections;

namespace NZ.Orz.Health;

public class TcpConnectionActiveHealthChecker : IActiveHealthChecker
{
    private readonly IConnectionFactory connectionFactory;

    public string Name => "Tcp";

    public TcpConnectionActiveHealthChecker(IConnectionFactory connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    public async Task CheckAsync(ActiveHealthCheckConfig config, DestinationState state, CancellationToken cancellationToken)
    {
        try
        {
            var c = await connectionFactory.ConnectAsync(state.EndPoint, cancellationToken);
            c.Abort();
            state.Health = DestinationHealth.Healthy;
        }
        catch (Exception ex)
        {
            state.Health = DestinationHealth.Unhealthy;
            //todo
        }
    }
}