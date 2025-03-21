using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Http.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZ.Orz.Http;

public class HttpDispatcher : IHttpDispatcher
{
    public async Task StartHttpAsync(ConnectionContext context, HttpConnectionDelegate next)
    {
        IRequestProcessor processor = null;
        switch (SelectProtocol(context))
        {
            case GatewayProtocols.HTTP1:
                processor = new HttpConnection1(context);
                break;

            case GatewayProtocols.HTTP2:
                break;

            default:
                throw new NotSupportedException($"{nameof(SelectProtocol)} returned something other than Http1, Http2.");
        }

        if (processor is not null)
        {
            await processor.ProcessRequestsAsync(next);
        }
    }

    public Task StartHttpAsync(MultiplexedConnectionContext c, HttpConnectionDelegate next)
    {
        throw new NotImplementedException();
    }

    private GatewayProtocols SelectProtocol(ConnectionContext context)
    {
        // todo check http2

        return GatewayProtocols.HTTP1;
    }
}