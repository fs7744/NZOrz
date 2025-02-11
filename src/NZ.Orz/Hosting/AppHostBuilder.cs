using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;

namespace NZ.Orz.Hosting;

public class AppHostBuilder : IAppHostBuilder
{
    private readonly List<Action<AppHostBuilderContext, IServiceCollection>> _configureServicesActions = new();
    private readonly List<IConfigureContainerAdapter> _configureContainerActions = new();
    private IServiceFactoryAdapter _serviceProviderFactory = new ServiceFactoryAdapter<IServiceCollection>(new DefaultServiceProviderFactory());
    private AppHostBuilderContext? _hostBuilderContext;
    private bool _hostBuilt;
    private IServiceProvider? _appServices;

    public IAppHostBuilder ConfigureServices(Action<AppHostBuilderContext, IServiceCollection> configureDelegate)
    {
        ArgumentNullException.ThrowIfNull(configureDelegate, nameof(configureDelegate));

        _configureServicesActions.Add(configureDelegate);
        return this;
    }

    public IAppHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<AppHostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory) where TContainerBuilder : notnull
    {
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));

        _serviceProviderFactory = new ServiceFactoryAdapter<TContainerBuilder>(() => _hostBuilderContext!, factory);
        return this;
    }

    public IAppHostBuilder ConfigureContainer<TContainerBuilder>(Action<AppHostBuilderContext, TContainerBuilder> configureDelegate)
    {
        ArgumentNullException.ThrowIfNull(configureDelegate, nameof(configureDelegate));

        _configureContainerActions.Add(new ConfigureContainerAdapter<TContainerBuilder>(configureDelegate));
        return this;
    }

    [MemberNotNull(nameof(_appServices))]
    internal static void PopulateServiceCollection(
            IServiceCollection services,
            AppHostBuilderContext hostBuilderContext,
            Func<IServiceProvider> serviceProviderGetter)
    {
        services.AddSingleton<IAppHost>(_ =>
        {
            IServiceProvider appServices = serviceProviderGetter();
            return new NZApp(appServices);
        });
        //services.AddOptions().Configure<HostOptions>(options => { options.Initialize(hostBuilderContext.Configuration); });
        //services.AddLogging();
    }

    [MemberNotNull(nameof(_appServices))]
    private void InitializeServiceProvider()
    {
        var services = new ServiceCollection();

        PopulateServiceCollection(
                services,
                _hostBuilderContext!,
                () => _appServices!);

        foreach (Action<AppHostBuilderContext, IServiceCollection> configureServicesAction in _configureServicesActions)
        {
            configureServicesAction(_hostBuilderContext!, services);
        }

        object containerBuilder = _serviceProviderFactory.CreateBuilder(services);

        foreach (IConfigureContainerAdapter containerAction in _configureContainerActions)
        {
            containerAction.ConfigureContainer(_hostBuilderContext!, containerBuilder);
        }

        _appServices = _serviceProviderFactory.CreateServiceProvider(containerBuilder);
    }

    public IAppHost Build()
    {
        if (_hostBuilt)
        {
            throw new InvalidOperationException("Build can only be called once.");
        }
        _hostBuilt = true;

        InitializeServiceProvider();

        if (_appServices is null)
        {
            throw new InvalidOperationException("The IServiceProviderFactory returned a null IServiceProvider");
        }

        //_ = _appServices.GetService<IConfiguration>();

        var host = _appServices.GetRequiredService<IAppHost>();

        return host;
    }
}