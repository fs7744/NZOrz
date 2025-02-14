namespace NZ.Orz.Connections;

public interface IMiddleware
{
    Task Invoke(ConnectionContext context, ConnectionDelegate next);
}