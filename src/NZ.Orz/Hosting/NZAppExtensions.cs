using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NZ.Orz.Config;
using NZ.Orz.Config.Configuration;

namespace NZ.Orz;

public static class NZAppExtensions
{
    public static IOrzApp UseJsonConfig(this IOrzApp app, string file = "appsettings.json", string section = "ReverseProxy")
    {
        ConfigurationRouteContractor.Section = section ?? "ReverseProxy";
        if (!"appsettings.json".Equals(file, StringComparison.OrdinalIgnoreCase))
        {
            app.ApplicationBuilder.Configuration.AddJsonFile(file, false, true);
        }
        app.ApplicationBuilder.Services.TryAddSingleton<IRouteContractor, ConfigurationRouteContractor>();
        return app;
    }
}