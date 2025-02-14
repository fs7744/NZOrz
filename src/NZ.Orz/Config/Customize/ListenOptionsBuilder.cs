using Microsoft.Extensions.DependencyInjection;
using NZ.Orz.Connections;
using System.Net;

namespace NZ.Orz.Config.Customize;

public class ListenOptionsBuilder
{
    private string key;
    public List<EndPoint> EndPoints { get; } = new List<EndPoint>();
    public List<Func<ConnectionDelegate, ConnectionDelegate>> Middlewares { get; } = new List<Func<ConnectionDelegate, ConnectionDelegate>>();
    public GatewayProtocols Protocols { get; set; }

    public ListenOptionsBuilder(string key)
    {
        this.key = key;
    }

    internal IServiceProvider ServiceProvider { get; set; }
    public IServiceCollection Services { get; internal set; }

    public ListenOptionsBuilder Listen(params EndPoint[] endPoints)
    {
        if (endPoints.Length == 0)
        {
            throw new ArgumentException("Can't be empty", nameof(endPoints));
        }
        EndPoints.AddRange(endPoints);
        return this;
    }

    internal ListenOptions Build()
    {
        ConnectionDelegate app = context =>
        {
            return Task.CompletedTask;
        };

        foreach (var component in Middlewares.Reverse<Func<ConnectionDelegate, ConnectionDelegate>>())
        {
            app = component(app);
        }

        return new ListenOptions() { Key = key, EndPoints = EndPoints, ConnectionDelegate = app, Protocols = Protocols };
    }

    public ListenOptionsBuilder UseMiddleware(Func<ConnectionDelegate, ConnectionDelegate> middleware)
    {
        Middlewares.Add(middleware);
        return this;
    }

    public ListenOptionsBuilder UseMiddleware<T>() where T : IMiddleware
    {
        UseMiddleware(next =>
        {
            var p = ServiceProvider.GetRequiredService<T>();
            return c => p.Invoke(c, next);
        });
        return this;
    }
}