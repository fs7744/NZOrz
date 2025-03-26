using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Connections.Exceptions;
using System.IO.Pipelines;
using System.Net;

namespace NZ.Orz.Http;

public abstract class HttpProtocol : HttpConnectionContext, IRequestProcessor
{
    public const int MaxExceptionDetailSize = 128;
    protected volatile bool _keepAlive = true;
    protected RequestProcessingStatus _requestProcessingStatus;
    protected string? _methodText;

    public HttpMethod Method { get; set; }
    public HttpVersion Version { get; set; }

    public bool IsUpgradableRequest { get; private set; }
    public bool IsUpgraded { get; set; }
    public bool IsExtendedConnectRequest { get; set; }
    public bool IsExtendedConnectAccepted { get; set; }
    public IPAddress? RemoteIpAddress { get; set; }
    public int RemotePort { get; set; }
    public IPAddress? LocalIpAddress { get; set; }
    public int LocalPort { get; set; }

    // https://datatracker.ietf.org/doc/html/rfc8441 ":protocol"
    public string? ConnectProtocol { get; set; }

    public string? Scheme { get; set; }
    public string? PathBase { get; set; }

    public string? Path { get; set; }
    public string? QueryString { get; set; }
    public string? RawTarget { get; set; }

    protected HttpProtocol(GatewayProtocols protocols, BaseConnectionContext connectionContext) : base(protocols, connectionContext)
    {
    }

    public async Task ProcessRequestsAsync(HttpConnectionDelegate application)
    {
        //todo
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
        _methodText = null;
        Method = HttpMethod.None;
        Version = HttpVersion.Unknown;
        PathBase = null;
        Path = null;
        RawTarget = null;
        QueryString = null;
        IsUpgraded = false;
        IsExtendedConnectRequest = false;
        IsExtendedConnectAccepted = false;
        ConnectProtocol = null;
        //_endpoint = null;
        //_httpProtocol = null;
        //IsWebTransportRequest = false;
        //_statusCode = StatusCodes.Status200OK;
        //_reasonPhrase = null;
        //todo
        OnReset();
    }

    protected abstract void OnReset();

    protected abstract void BeginRequestProcessing();

    protected abstract bool BeginRead(out ValueTask<ReadResult> awaitable);

    protected abstract bool TryParseRequest(ReadResult result, out bool endConnection);

    protected abstract MessageBody CreateMessageBody();

    internal abstract void DisableKeepAlive(ConnectionEndReason reason);

    public abstract void StopProcessingNextRequest(ConnectionEndReason reason);

    public abstract void HandleRequestHeadersTimeout();

    public abstract void HandleReadDataRateTimeout();

    public abstract void OnInputOrOutputCompleted();

    public abstract void Tick(long timestamp);

    public abstract void Abort(ConnectionAbortedException ex, ConnectionEndReason reason);
}