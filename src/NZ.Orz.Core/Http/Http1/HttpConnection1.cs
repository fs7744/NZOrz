using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Connections.Exceptions;
using NZ.Orz.Http.Exceptions;
using NZ.Orz.Servers;
using System;
using System.Buffers;
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
    private volatile bool _requestTimedOut;

    public HttpConnection1(ConnectionContext connectionContext, ServiceContext serviceContext, TimeoutControl timeoutControl) : base(GatewayProtocols.HTTP1, connectionContext)
    {
        Input = connectionContext.Transport.Input;
        this.serviceContext = serviceContext;
        TimeoutControl = timeoutControl;
    }

    public PipeReader Input { get; }
    public TimeoutControl TimeoutControl { get; }
    public bool RequestTimedOut => _requestTimedOut;

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
        _requestTimedOut = false;
        // todo
    }

    protected override bool TryParseRequest(ReadResult result, out bool endConnection)
    {
        var reader = new SequenceReader<byte>(result.Buffer);
        var isConsumed = false;
        try
        {
            isConsumed = ParseRequest(ref reader);
        }
        catch (InvalidOperationException) when (_requestProcessingStatus == RequestProcessingStatus.ParsingHeaders)
        {
            throw BadHttpRequestException.GetException(RequestRejectionReason.MalformedRequestInvalidHeaders);
            throw;
        }
        catch (BadHttpRequestException ex)
        {
            //OnBadRequest(result.Buffer, ex);
            throw;
        }
        catch (Exception)
        {
            //KestrelMetrics.AddConnectionEndReason(MetricsContext, ConnectionEndReason.OtherError);
            throw;
        }
        finally
        {
            Input.AdvanceTo(reader.Position, isConsumed ? reader.Position : result.Buffer.End);
        }

        if (result.IsCompleted)
        {
            switch (_requestProcessingStatus)
            {
                case RequestProcessingStatus.RequestPending:
                    endConnection = true;
                    return true;

                case RequestProcessingStatus.ParsingRequestLine:
                    throw BadHttpRequestException.GetException(RequestRejectionReason.InvalidRequestLine);
                case RequestProcessingStatus.ParsingHeaders:
                    throw BadHttpRequestException.GetException(RequestRejectionReason.MalformedRequestInvalidHeaders);
            }
        }
        else if (!_keepAlive && _requestProcessingStatus == RequestProcessingStatus.RequestPending)
        {
            // Stop the request processing loop if the server is shutting down or there was a keep-alive timeout
            // and there is no ongoing request.
            endConnection = true;
            return true;
        }
        else if (RequestTimedOut)
        {
            // In this case, there is an ongoing request but the start line/header parsing has timed out, so send
            // a 408 response.
            throw BadHttpRequestException.GetException(RequestRejectionReason.RequestHeadersTimeout);
        }

        endConnection = false;
        if (_requestProcessingStatus == RequestProcessingStatus.AppStarted)
        {
            EnsureHostHeaderExists();
            return true;
        }
        else
        {
            return false;
        }
    }

    private void EnsureHostHeaderExists()
    {
        // todo
    }

    private bool ParseRequest(ref SequenceReader<byte> reader)
    {
        throw new NotImplementedException();
    }

    internal override void DisableKeepAlive(ConnectionEndReason reason)
    {
        throw new NotImplementedException();
    }

    public override void Abort(ConnectionAbortedException ex, ConnectionEndReason reason)
    {
        throw new NotImplementedException();
    }

    public override void HandleReadDataRateTimeout()
    {
        throw new NotImplementedException();
    }

    public override void HandleRequestHeadersTimeout()
    {
        throw new NotImplementedException();
    }

    public override void OnInputOrOutputCompleted()
    {
        throw new NotImplementedException();
    }

    public override void StopProcessingNextRequest(ConnectionEndReason reason)
    {
        throw new NotImplementedException();
    }

    public override void Tick(long timestamp)
    {
        throw new NotImplementedException();
    }
}