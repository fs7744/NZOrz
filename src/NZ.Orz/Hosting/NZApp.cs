namespace NZ.Orz.Hosting;

public class NZApp : IAppHost
{
    public NZApp(IServiceProvider appServices)
    {
        this.Services = appServices;
    }

    public static IAppHostBuilder CreateBuilder(string[] args = null)
    {
        AppHostBuilder builder = new();
        return builder.ConfigureDefaults(args);
    }

    public IServiceProvider Services { get; internal set; }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}