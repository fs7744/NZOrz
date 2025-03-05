using NZ.Orz.Connections;
using NZ.Orz.ReverseProxy.L4;

public class EchoMiddleware : ITcpMiddleware
{
    public int Order => 0;

    public Task<ReadOnlyMemory<byte>> OnRequest(ConnectionContext connection, ReadOnlyMemory<byte> source, CancellationToken cancellationToken, TcpConnectionDelegate next)
    {
        Console.WriteLine($"{connection.LocalEndPoint.ToString()} request size: {source.Length}");
        return Task.FromResult(source);
    }

    public Task<ReadOnlyMemory<byte>> OnResponse(ConnectionContext connection, ReadOnlyMemory<byte> source, CancellationToken cancellationToken, TcpConnectionDelegate next)
    {
        Console.WriteLine($"{connection.SelectedDestination.EndPoint.ToString()} reponse size: {source.Length}");
        return Task.FromResult(source);
    }
}