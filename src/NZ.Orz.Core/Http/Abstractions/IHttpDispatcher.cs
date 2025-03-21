using NZ.Orz.Connections;

namespace NZ.Orz.Http;

public interface IHttpDispatcher
{
    Task StartHttpAsync(ConnectionContext context, HttpConnectionDelegate next);

    Task StartHttpAsync(MultiplexedConnectionContext c, HttpConnectionDelegate next);
}