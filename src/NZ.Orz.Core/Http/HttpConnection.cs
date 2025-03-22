using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Servers;
using System;

namespace NZ.Orz.Http;

public class HttpConnection : ITimeoutHandler
{
    private readonly BaseConnectionContext context;
    private readonly ServiceContext serviceContext;
    private readonly TimeoutControl timeoutControl;

    public HttpConnection(ConnectionContext context, ServiceContext serviceContext)
    {
        this.context = context;
        this.serviceContext = serviceContext;
        this.timeoutControl = new TimeoutControl(this, serviceContext.TimeProvider);
    }

    public HttpConnection(MultiplexedConnectionContext context, ServiceContext serviceContext)
    {
        this.context = context;
        this.serviceContext = serviceContext;
        this.timeoutControl = new TimeoutControl(this, serviceContext.TimeProvider);
    }

    public async Task StartHttpAsync(HttpConnectionDelegate next)
    {
        timeoutControl.Initialize();
        IRequestProcessor processor = null;
        switch (SelectProtocol())
        {
            case GatewayProtocols.HTTP1:
                processor = new HttpConnection1((ConnectionContext)context, serviceContext, timeoutControl);
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

        //TODO
    }

    private GatewayProtocols SelectProtocol()
    {
        if (context is ConnectionContext c)
        {
            // todo check http2

            return GatewayProtocols.HTTP1;
        }
        else
        {
            // todo check http3

            return GatewayProtocols.HTTP3;
        }
    }

    public void OnTimeout(TimeoutReason reason)
    {
        throw new NotImplementedException();
    }
}