using Microsoft.Extensions.DependencyInjection;

namespace NZ.Orz.Hosting;

public class NZApp : IAppHost
{
    private IEnumerable<IHostedService>? _hostedServices;
    private volatile bool _stopCalled;

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

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        using var combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _applicationLifetime.ApplicationStopping);
        CancellationToken combinedCancellationToken = combinedCancellationTokenSource.Token;

        await _hostLifetime.WaitForStartAsync(combinedCancellationToken).ConfigureAwait(false);

        combinedCancellationToken.ThrowIfCancellationRequested();
        _hostedServices = Services.GetRequiredService<IEnumerable<IHostedService>>();

        foreach (IHostedService hostedService in _hostedServices)
        {
            await hostedService.StartAsync(combinedCancellationToken).ConfigureAwait(false);
        }

        _applicationLifetime.NotifyStarted();
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _stopCalled = true;

        using (var cts = new CancellationTokenSource(_options.ShutdownTimeout))
        using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken))
        {
            CancellationToken token = linkedCts.Token;
            // Trigger IHostApplicationLifetime.ApplicationStopping
            _applicationLifetime.StopApplication();

            var exceptions = new List<Exception>();
            if (_hostedServices != null) // Started?
            {
                foreach (IHostedService hostedService in _hostedServices.Reverse())
                {
                    try
                    {
                        await hostedService.StopAsync(token).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }
            }

            // Fire IHostApplicationLifetime.Stopped
            _applicationLifetime.NotifyStopped();

            try
            {
                await _hostLifetime.StopAsync(token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            if (exceptions.Count > 0)
            {
                var ex = new AggregateException("One or more hosted services failed to stop.", exceptions);
                throw ex;
            }
        }
    }

    public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(Services).ConfigureAwait(false);

        static async ValueTask DisposeAsync(object o)
        {
            switch (o)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                    break;

                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }
    }
}
}