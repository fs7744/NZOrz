using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NZ.Orz;

public interface IOrzApp
{
    public HostApplicationBuilder ApplicationBuilder { get; }

    public IServiceCollection Services { get => ApplicationBuilder.Services; }

    IHost Build();
}

internal class OrzApp : IOrzApp
{
    public OrzApp(HostApplicationBuilder builder)
    {
        ApplicationBuilder = builder;
    }

    public HostApplicationBuilder ApplicationBuilder { get; private set; }

    public IHost Build()
    {
        var app = ApplicationBuilder.Build();
        ApplicationBuilder = null;
        return app;
    }
}