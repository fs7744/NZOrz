namespace NZ.Orz.Http;

public abstract class MessageBody
{
    private readonly HttpProtocol _context;

    protected MessageBody(HttpProtocol context)
    {
        _context = context;
    }

    public HttpProtocol Context => _context;
    public bool RequestKeepAlive { get; protected set; }
}