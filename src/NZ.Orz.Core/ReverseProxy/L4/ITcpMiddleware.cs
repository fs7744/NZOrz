using NZ.Orz.Connections;

namespace NZ.Orz.ReverseProxy.L4;

public interface IProxyMiddleware
{
    int Order { get; }

    Task<ReadOnlyMemory<byte>> OnRequest(ConnectionContext connection, ReadOnlyMemory<byte> source, CancellationToken cancellationToken, ProxyConnectionDelegate next);

    Task<ReadOnlyMemory<byte>> OnResponse(ConnectionContext connection, ReadOnlyMemory<byte> source, CancellationToken cancellationToken, ProxyConnectionDelegate next);
}

public interface ITcpMiddleware : IProxyMiddleware
{
}

public interface IUdpMiddleware : IProxyMiddleware
{
}