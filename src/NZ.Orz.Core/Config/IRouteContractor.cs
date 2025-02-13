namespace NZ.Orz.Config;

public interface IRouteContractor
{
    Task LoadAsync(CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);
}