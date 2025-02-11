using Microsoft.Extensions.DependencyInjection;

namespace NZ.Orz.Hosting;

internal interface IServiceFactoryAdapter
{
    object CreateBuilder(IServiceCollection services);

    IServiceProvider CreateServiceProvider(object containerBuilder);
}