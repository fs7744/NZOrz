using Microsoft.Extensions.DependencyInjection;

namespace NZ.Orz.Hosting;

internal sealed class ServiceFactoryAdapter<TContainerBuilder> : IServiceFactoryAdapter
{
    private IServiceProviderFactory<TContainerBuilder> _serviceProviderFactory;

    private readonly Func<AppHostBuilderContext> _contextResolver;

    private readonly Func<AppHostBuilderContext, IServiceProviderFactory<TContainerBuilder>> _factoryResolver;

    public ServiceFactoryAdapter(IServiceProviderFactory<TContainerBuilder> serviceProviderFactory)
    {
        ArgumentNullException.ThrowIfNull(serviceProviderFactory, nameof(serviceProviderFactory));
        _serviceProviderFactory = serviceProviderFactory;
    }

    public ServiceFactoryAdapter(Func<AppHostBuilderContext> contextResolver, Func<AppHostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factoryResolver)
    {
        ArgumentNullException.ThrowIfNull(contextResolver, nameof(contextResolver));
        ArgumentNullException.ThrowIfNull(factoryResolver, nameof(factoryResolver));
        _contextResolver = contextResolver;
        _factoryResolver = factoryResolver;
    }

    public object CreateBuilder(IServiceCollection services)
    {
        if (_serviceProviderFactory == null)
        {
            _serviceProviderFactory = _factoryResolver(_contextResolver());
            if (_serviceProviderFactory == null)
            {
                throw new InvalidOperationException("The resolver returned a null IServiceProviderFactory");
            }
        }

        return _serviceProviderFactory.CreateBuilder(services);
    }

    public IServiceProvider CreateServiceProvider(object containerBuilder)
    {
        if (_serviceProviderFactory == null)
        {
            throw new InvalidOperationException("CreateBuilder must be called before CreateServiceProvider");
        }

        return _serviceProviderFactory.CreateServiceProvider((TContainerBuilder)containerBuilder);
    }
}