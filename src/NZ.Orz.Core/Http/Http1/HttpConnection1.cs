using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Servers;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZ.Orz.Http;

public class HttpConnection1 : HttpProtocol
{
    private readonly ServiceContext serviceContext;
    private uint _requestCount;

    public HttpConnection1(ConnectionContext connectionContext, ServiceContext serviceContext, TimeoutControl timeoutControl) : base(GatewayProtocols.HTTP1, connectionContext)
    {
        Input = connectionContext.Transport.Input;
        this.serviceContext = serviceContext;
        TimeoutControl = timeoutControl;
    }

    public PipeReader Input { get; }
    public TimeoutControl TimeoutControl { get; }

    protected override bool BeginRead(out ValueTask<ReadResult> awaitable)
    {
        awaitable = Input.ReadAsync();
        return true;
    }

    protected override void BeginRequestProcessing()
    {
        Reset();
        _requestCount++;
        TimeoutControl.SetTimeout(serviceContext.ServerOptions.Limits.KeepAliveTimeout, TimeoutReason.KeepAlive);
    }

    protected override MessageBody CreateMessageBody()
    {
        throw new NotImplementedException();
    }

    protected override void OnReset()
    {
        throw new NotImplementedException();
    }

    protected override bool TryParseRequest(ReadResult result, out bool endConnection)
    {
        throw new NotImplementedException();
    }

    internal override void DisableKeepAlive(ConnectionEndReason reason)
    {
        throw new NotImplementedException();
    }
}