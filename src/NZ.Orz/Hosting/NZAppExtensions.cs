using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NZ.Orz.Config;
using NZ.Orz.Config.Memory;
using NZ.Orz.Config.Configuration;

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
        var r = builder.Build();
        app.ApplicationBuilder.Services.AddSingleton<IRouteContractor>(i =>
        {
            r.ServiceProvider = i;
            return r;
        });
        return app;
    }

    public static IOrzApp UseJsonConfig(this IOrzApp app, string file = "appsettings.json", string section = "ReverseProxy")
    {
        ConfigurationRouteContractor.Section = section ?? "ReverseProxy";
        if (File.Exists(file))
        {
            var appSettingsJson = File.ReadAllBytes(file);
            app.ApplicationBuilder.Configuration.Add<JsonStreamConfigurationSource>(s => s.Stream = new MemoryStream(appSettingsJson));
        }
        app.ApplicationBuilder.Services.AddSingleton<IRouteContractor, ConfigurationRouteContractor>();
        return app;
    }
}