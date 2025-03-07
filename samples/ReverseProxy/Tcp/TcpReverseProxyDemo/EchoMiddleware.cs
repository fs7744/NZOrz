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
        //source = Encoding.UTF8.GetBytes("HTTP/1.1 400 Bad Request\r\nDate: Sun, 18 Oct 2012 10:36:20 GMT\r\nServer: Apache/2.2.14 (Win32)\r\nContent-Length: 0\r\nContent-Type: text/html; charset=iso-8859-1\r\nConnection: Closed\r\n\r\n").AsMemory();
        //connection.Abort();
        return Task.FromResult(source);
    }
}