using NZ.Orz.Config;
using NZ.Orz.Connections;
using System.Runtime.CompilerServices;

namespace NZ.Orz.Health;

public class TcpConnectionActiveHealthChecker : IActiveHealthChecker
{
    private readonly ConditionalWeakTable<DestinationState, ActiveHistory> histories = new ConditionalWeakTable<DestinationState, ActiveHistory>();
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
            SetStatus(config, state, false);
            state.Health = DestinationHealth.Healthy;
        }
        catch (Exception ex)
        {
            SetStatus(config, state, true);
            state.Health = DestinationHealth.Unhealthy;
            //todo
        }
    }

    private void SetStatus(ActiveHealthCheckConfig config, DestinationState state, bool isFailed)
    {
        var h = histories.GetOrCreateValue(state);
        if (isFailed)
        {
            h.Fails++;
            if (h.Fails >= config.Fails)
            {
                state.Health = DestinationHealth.Unhealthy;
                h.Fails = 0;
                h.Passes = 0;
            }
        }
        else
        {
            h.Passes++;
            if (h.Passes >= config.Passes)
            {
                state.Health = DestinationHealth.Healthy;
                h.Fails = 0;
                h.Passes = 0;
            }
        }
    }

    public sealed class ActiveHistory
    {
        public int Passes { get; set; }

        public int Fails { get; set; }
    }
}