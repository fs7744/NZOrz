using NZ.Orz.Connections;

namespace NZ.Orz.ReverseProxy.L4;

public delegate Task<ReadOnlyMemory<byte>> TcpConnectionDelegate(ConnectionContext connection, ReadOnlyMemory<byte> source, CancellationToken cancellationToken);