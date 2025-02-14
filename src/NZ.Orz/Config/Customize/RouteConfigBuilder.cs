using Microsoft.Extensions.DependencyInjection;

namespace NZ.Orz.Config.Customize;

public class RouteConfigBuilder
{
    public IServiceCollection Services { get; internal set; }
    internal List<ListenOptionsBuilder> EndPoints { get; private set; } = new List<ListenOptionsBuilder>();

    public void AddEndPoint(string key, Action<ListenOptionsBuilder> action)
    {
        var builder = new ListenOptionsBuilder(key);
        builder.Services = Services;
        EndPoints.Add(builder);
        action(builder);
    }
}