using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZ.Orz.Hosting;

public static class HostExtensions
{
    public static IAppHostBuilder ConfigureDefaults(this IAppHostBuilder builder, string[]? args)
    {
        return builder
                      //.ConfigureHostConfiguration(config => ApplyDefaultHostConfiguration(config, args))
                      //          .ConfigureAppConfiguration((hostingContext, config) => ApplyDefaultAppConfiguration(hostingContext, config, args))
                      .ConfigureServices(AddDefaultServices)
                      .UseServiceProviderFactory(context => new DefaultServiceProviderFactory());
    }

    private static void AddDefaultServices(AppHostBuilderContext context, IServiceCollection services)
    {
        //        services.AddLogging(logging =>
        //        {
        //            bool isWindows =
        //#if NETCOREAPP
        //                OperatingSystem.IsWindows();
        //#else
        //                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        //#endif

        //            logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
        //#if NETCOREAPP
        //            if (!OperatingSystem.IsBrowser())
        //#endif
        //            {
        //                logging.AddConsole();
        //            }
        //            logging.AddDebug();
        //            logging.AddEventSourceLogger();

        //            if (isWindows)
        //            {
        //                // Add the EventLogLoggerProvider on windows machines
        //                logging.AddEventLog();
        //            }

        //            logging.Configure(options =>
        //            {
        //                options.ActivityTrackingOptions =
        //                    ActivityTrackingOptions.SpanId |
        //                    ActivityTrackingOptions.TraceId |
        //                    ActivityTrackingOptions.ParentId;
        //            });
        //        });
    }
}