using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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