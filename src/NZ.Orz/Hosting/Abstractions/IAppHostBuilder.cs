using Microsoft.Extensions.DependencyInjection;

namespace NZ.Orz.Hosting;

public interface IAppHostBuilder
{
    IAppHost Build();

    IAppHostBuilder ConfigureServices(Action<AppHostBuilderContext, IServiceCollection> configureDelegate);

    IAppHostBuilder ConfigureContainer<TContainerBuilder>(Action<AppHostBuilderContext, TContainerBuilder> configureDelegate);

    IAppHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<AppHostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory) where TContainerBuilder : notnull;
}