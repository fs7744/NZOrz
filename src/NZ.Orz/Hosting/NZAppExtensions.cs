using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NZ.Orz.Config;
using NZ.Orz.Config.Configuration;

namespace NZ.Orz;

public static class NZAppExtensions
{
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