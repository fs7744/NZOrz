using NZ.Orz.Config;
using NZ.Orz.Connections;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZ.Orz.Http;

public abstract class HttpProtocol : HttpConnectionContext, IRequestProcessor
{
    private bool _keepAlive;
    protected RequestProcessingStatus _requestProcessingStatus;

    protected HttpProtocol(GatewayProtocols protocols, BaseConnectionContext connectionContext) : base(protocols, connectionContext)
    {
    }

    public async Task ProcessRequestsAsync(HttpConnectionDelegate application)
    {
        try
        {
            await DoProcessRequestsAsync(application);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
        }
    }

    public async Task DoProcessRequestsAsync(HttpConnectionDelegate application)
    {
        while (_keepAlive)
        {
            BeginRequestProcessing();
            var result = default(ReadResult);
            bool endConnection;
            do
            {
                if (BeginRead(out var awaitable))
                {
                    result = await awaitable;
                }
            } while (!TryParseRequest(result, out endConnection));

            if (endConnection)
            {
                // Connection finished, stop processing requests
                return;
            }

            var messageBody = CreateMessageBody();
            if (!messageBody.RequestKeepAlive)
            {
                DisableKeepAlive(ConnectionEndReason.RequestNoKeepAlive);
            }
            //todo
        }
    }

    public void Reset()
    {
        _requestProcessingStatus = RequestProcessingStatus.RequestPending;
        //todo
        OnReset();
    }

    protected abstract void OnReset();

    protected abstract void BeginRequestProcessing();

    protected abstract bool BeginRead(out ValueTask<ReadResult> awaitable);

    protected abstract bool TryParseRequest(ReadResult result, out bool endConnection);

    protected abstract MessageBody CreateMessageBody();

    internal abstract void DisableKeepAlive(ConnectionEndReason reason);
}