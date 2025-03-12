using NZ.Orz.Connections;
using NZ.Orz.ReverseProxy.L4;

public class UdpEchoMiddleware : IUdpMiddleware
{
    public int Order => 0;

    public Task<ReadOnlyMemory<byte>> OnRequest(ConnectionContext connection, ReadOnlyMemory<byte> source, CancellationToken cancellationToken, ProxyConnectionDelegate next)
    {
        Console.WriteLine($"udp {DateTime.Now} {connection.LocalEndPoint.ToString()} request size: {source.Length}");
        return Task.FromResult(source);
    }

    public Task<ReadOnlyMemory<byte>> OnResponse(ConnectionContext connection, ReadOnlyMemory<byte> source, CancellationToken cancellationToken, ProxyConnectionDelegate next)
    {
        Console.WriteLine($"udp {DateTime.Now} {connection.SelectedDestination.EndPoint.ToString()} reponse size: {source.Length}");
        return Task.FromResult(source);
    }
}