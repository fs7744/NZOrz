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
    private const byte ByteCR = (byte)'\r';
    private const byte ByteLF = (byte)'\n';
    private const byte ByteAsterisk = (byte)'*';
    private const byte ByteForwardSlash = (byte)'/';
    private const string Asterisk = "*";
    private const string ForwardSlash = "/";

    private readonly ServiceContext serviceContext;
    private uint _requestCount;
    private volatile bool _requestTimedOut;
    private ServerLimits limits;
    private long _remainingRequestHeadersBytesAllowed;

    public HttpConnection1(ConnectionContext connectionContext, ServiceContext serviceContext, TimeoutControl timeoutControl) : base(GatewayProtocols.HTTP1, connectionContext)
    {
        Input = connectionContext.Transport.Input;
        this.serviceContext = serviceContext;
        TimeoutControl = timeoutControl;
        this.limits = serviceContext.ServerOptions.Limits;
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
        TimeoutControl.SetTimeout(limits.KeepAliveTimeout, TimeoutReason.KeepAlive);
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
        switch (_requestProcessingStatus)
        {
            case RequestProcessingStatus.RequestPending:
                // Skip any empty lines (\r or \n) between requests.
                // Peek first as a minor performance optimization; it's a quick inlined check.
                if (reader.TryPeek(out byte b) && (b == ByteCR || b == ByteLF))
                {
                    reader.AdvancePastAny(ByteCR, ByteLF);
                }

                if (reader.End)
                {
                    break;
                }

                TimeoutControl.ResetTimeout(limits.RequestHeadersTimeout, TimeoutReason.RequestHeaders);

                _requestProcessingStatus = RequestProcessingStatus.ParsingRequestLine;
                goto case RequestProcessingStatus.ParsingRequestLine;
            case RequestProcessingStatus.ParsingRequestLine:
                if (TakeStartLine(ref reader))
                {
                    _requestProcessingStatus = RequestProcessingStatus.ParsingHeaders;
                    goto case RequestProcessingStatus.ParsingHeaders;
                }
                else
                {
                    break;
                }
            case RequestProcessingStatus.ParsingHeaders:
                if (TakeMessageHeaders(ref reader, trailers: false))
                {
                    _requestProcessingStatus = RequestProcessingStatus.AppStarted;
                    // Consumed preamble
                    return true;
                }
                break;
        }

        // Haven't completed consuming preamble
        return false;
    }

    public bool TakeStartLine(ref SequenceReader<byte> reader)
    {
        // Make sure the buffer is limited
        if (reader.Remaining >= limits.MaxRequestLineSize)
        {
            // Input oversize, cap amount checked
            return TrimAndTakeStartLine(ref reader);
        }

        return ParseRequestLine(ref reader);

        bool TrimAndTakeStartLine(ref SequenceReader<byte> reader)
        {
            var trimmedBuffer = reader.Sequence.Slice(reader.Position, limits.MaxRequestLineSize);
            var trimmedReader = new SequenceReader<byte>(trimmedBuffer);

            if (!ParseRequestLine(ref trimmedReader))
            {
                // We read the maximum allowed but didn't complete the start line.
                throw BadHttpRequestException.GetException(RequestRejectionReason.RequestLineTooLong);
            }

            reader.Advance(trimmedReader.Consumed);
            return true;
        }
    }

    public bool TakeMessageHeaders(ref SequenceReader<byte> reader, bool trailers)
    {
        // Make sure the buffer is limited
        if (reader.Remaining > _remainingRequestHeadersBytesAllowed)
        {
            // Input oversize, cap amount checked
            return TrimAndTakeMessageHeaders(ref reader, trailers);
        }

        var alreadyConsumed = reader.Consumed;

        try
        {
            var result = ParseHeaders(ref reader);
            if (result)
            {
                TimeoutControl.CancelTimeout();
            }

            return result;
        }
        finally
        {
            _remainingRequestHeadersBytesAllowed -= reader.Consumed - alreadyConsumed;
        }

        bool TrimAndTakeMessageHeaders(ref SequenceReader<byte> reader, bool trailers)
        {
            var trimmedBuffer = reader.Sequence.Slice(reader.Position, _remainingRequestHeadersBytesAllowed);
            var trimmedReader = new SequenceReader<byte>(trimmedBuffer);
            try
            {
                if (!ParseHeaders(trailers, ref trimmedReader))
                {
                    // We read the maximum allowed but didn't complete the headers.
                    throw BadHttpRequestException.GetException(RequestRejectionReason.HeadersExceedMaxTotalSize);
                }

                TimeoutControl.CancelTimeout();

                reader.Advance(trimmedReader.Consumed);

                return true;
            }
            finally
            {
                _remainingRequestHeadersBytesAllowed -= trimmedReader.Consumed;
            }
        }
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

    #region HttpParser

    public bool ParseRequestLine(ref SequenceReader<byte> reader)
    {
        throw new NotImplementedException();
    }

    #endregion HttpParser
}