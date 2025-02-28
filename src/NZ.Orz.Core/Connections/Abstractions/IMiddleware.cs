namespace NZ.Orz.Connections;

public interface IMiddleware
{
    Task Invoke(ConnectionContext context, ConnectionDelegate next);
}

public interface IOrderMiddleware : IMiddleware
{
    int Order { get; }

    Task Invoke(ConnectionContext context, ConnectionDelegate next);
}