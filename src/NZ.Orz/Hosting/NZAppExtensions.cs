using Microsoft.Extensions.DependencyInjection;
using NZ.Orz.Config;
using NZ.Orz.Config.Memory;

namespace NZ.Orz;

public static class NZAppExtensions
{
    /// <summary>
    /// MemoryConfig is for test, it can't change config when app running
    /// </summary>
    public static IOrzApp UseMemoryConfig(this IOrzApp app, Action<MemoryReverseProxyConfigBuilder> action)
    {
        var builder = new MemoryReverseProxyConfigBuilder();
        action?.Invoke(builder);
        app.ApplicationBuilder.Services.AddSingleton<IRouteContractor>(builder.Build());
        return app;
    }
}