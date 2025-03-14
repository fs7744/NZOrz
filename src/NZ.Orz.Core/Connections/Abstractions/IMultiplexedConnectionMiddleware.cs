namespace NZ.Orz.Connections;

public interface IMultiplexedConnectionMiddleware
{
    Task Invoke(MultiplexedConnectionContext context, MultiplexedConnectionDelegate next);
}

public interface IOrderMultiplexedConnectionMiddleware : IMultiplexedConnectionMiddleware
{
    int Order { get; }
}