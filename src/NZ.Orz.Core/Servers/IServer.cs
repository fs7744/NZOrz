namespace NZ.Orz.Servers;

public interface IServer
{
    Task StartAsync(CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);
}