namespace NZ.Orz.Config;

public interface IRouteContractor
{
    ServerOptions GetServerOptions();

    Task LoadAsync(CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);
}