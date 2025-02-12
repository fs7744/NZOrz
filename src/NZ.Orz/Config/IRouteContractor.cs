namespace NZ.Orz.Config;

public interface IRouteContractor
{
    Task<RouteConfig> LoadAllAsync(CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);
}