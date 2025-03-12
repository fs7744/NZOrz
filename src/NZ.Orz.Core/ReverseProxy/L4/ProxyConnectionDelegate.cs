using NZ.Orz.Connections;

namespace NZ.Orz.ReverseProxy.L4;

public delegate Task<ReadOnlyMemory<byte>> ProxyConnectionDelegate(ConnectionContext connection, ReadOnlyMemory<byte> source, CancellationToken cancellationToken);