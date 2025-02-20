namespace NZ.Orz.Connections;

public interface IMultiplexedConnectionMiddleware
{
    Task Invoke(MultiplexedConnectionContext context, MultiplexedConnectionDelegate next);
}