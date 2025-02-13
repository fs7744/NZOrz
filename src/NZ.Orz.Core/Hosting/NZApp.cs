using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NZ.Orz.Hosting;

namespace NZ.Orz;

public static partial class NZApp
{
    public static HostApplicationBuilder CreateBuilder(string[] args = null)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddSingleton<IHostedService, HostedService>();
        return builder;
    }
}