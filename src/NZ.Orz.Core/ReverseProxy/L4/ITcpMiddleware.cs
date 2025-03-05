using NZ.Orz.Connections;

namespace NZ.Orz.ReverseProxy.L4;

public interface ITcpMiddleware
{
    int Order { get; }

    Task<ReadOnlyMemory<byte>> OnRequest(ConnectionContext connection, ReadOnlyMemory<byte> source, CancellationToken cancellationToken, TcpConnectionDelegate next);

    Task<ReadOnlyMemory<byte>> OnResponse(ConnectionContext connection, ReadOnlyMemory<byte> source, CancellationToken cancellationToken, TcpConnectionDelegate next);
}