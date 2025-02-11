namespace NZ.Orz.Hosting;

internal sealed class ConfigureContainerAdapter<TContainerBuilder> : IConfigureContainerAdapter
{
    private readonly Action<AppHostBuilderContext, TContainerBuilder> _action;

    public ConfigureContainerAdapter(Action<AppHostBuilderContext, TContainerBuilder> action)
    {
        ArgumentNullException.ThrowIfNull(action, nameof(action));
        _action = action;
    }

    public void ConfigureContainer(AppHostBuilderContext hostContext, object containerBuilder)
    {
        _action(hostContext, (TContainerBuilder)containerBuilder);
    }
}