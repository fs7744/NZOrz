namespace NZ.Orz.Hosting;

public interface IAppHost : IDisposable
{
    IServiceProvider Services { get; }

    Task StartAsync(CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);
}