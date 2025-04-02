using Microsoft.Extensions.Primitives;
using Microsoft.VisualBasic;
using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Connections.Exceptions;
using NZ.Orz.Http.Exceptions;
using NZ.Orz.Metrics;
using NZ.Orz.Servers;
using System;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net;
using System.Net.Http.Headers;

namespace NZ.Orz.Http;

public abstract class HttpProtocol : HttpConnectionContext, IRequestProcessor
{
    public const int MaxExceptionDetailSize = 128;
    protected volatile bool _keepAlive = true;
    protected RequestProcessingStatus _requestProcessingStatus;
    protected string? _methodText;
    protected int _requestHeadersParsed;
    protected Exception? _applicationException;
    private BadHttpRequestException? _requestRejectedException;
    private string? _requestId;
    private Stream? _requestStreamInternal;
    private Stream? _responseStreamInternal;

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

    public HttpRequestHeaders RequestHeaders { get; set; } = new HttpRequestHeaders();

    public Stream RequestBody { get; set; } = default!;
    public PipeReader RequestBodyPipeReader { get; set; } = default!;
    public IHeaderDictionary ResponseHeaders { get; set; } = default!;
    public Stream ResponseBody { get; set; } = default!;
    public PipeWriter ResponseBodyPipeWriter { get; set; } = default!;
    public ServiceContext ServiceContext { get; }
    public bool HasStartedConsumingRequestBody { get; set; }
    public MinDataRate? MinRequestBodyDataRate { get; set; }
    public long? MaxRequestBodySize { get; set; }
    public ITimeoutControl TimeoutControl { get; }
    public string ConnectionId => ConnectionContext.ConnectionId;

    public string TraceIdentifier
    {
        set => _requestId = value;
        get
        {
            // don't generate an ID until it is requested
            if (_requestId == null)
            {
                _requestId = CreateRequestId();
            }
            return _requestId;
        }
    }

    protected OrzLogger Log => ServiceContext.Log;

    public bool HasResponseStarted => _requestProcessingStatus >= RequestProcessingStatus.HeadersCommitted;

    public bool HasFlushedHeaders => _requestProcessingStatus >= RequestProcessingStatus.HeadersFlushed;

    public bool HasResponseCompleted => _requestProcessingStatus == RequestProcessingStatus.ResponseCompleted;

    protected HttpProtocol(GatewayProtocols protocols, BaseConnectionContext connectionContext, ServiceContext serviceContext, ITimeoutControl timeoutControl) : base(protocols, connectionContext)
    {
        ServiceContext = serviceContext;
        TimeoutControl = timeoutControl;
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
        _requestHeadersParsed = 0;
        HasStartedConsumingRequestBody = false;
        MinRequestBodyDataRate = ServiceContext.ServerOptions.Limits.MinRequestBodyDataRate;
        MaxRequestBodySize = ServiceContext.ServerOptions.Limits.MaxRequestBodySize;
        _applicationException = null;
        _requestRejectedException = null;
        TraceIdentifier = null!;
        //_endpoint = null;
        //_httpProtocol = null;
        //IsWebTransportRequest = false;
        //_statusCode = StatusCodes.Status200OK;
        //_reasonPhrase = null;
        RequestHeaders?.Clear();
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

    protected abstract string CreateRequestId();

    public void ReportApplicationError(Exception? ex)
    {
        // ReportApplicationError can be called with a null exception from MessageBody
        if (ex == null)
        {
            return;
        }

        if (_applicationException == null)
        {
            _applicationException = ex;
        }
        else if (_applicationException is AggregateException)
        {
            _applicationException = new AggregateException(_applicationException, ex).Flatten();
        }
        else
        {
            _applicationException = new AggregateException(_applicationException, ex);
        }

        Log.ApplicationError(ConnectionId, TraceIdentifier, ex);
    }

    public void SetBadRequestState(BadHttpRequestException ex)
    {
        Log.ConnectionBadRequest(ConnectionId, ex);
        _requestRejectedException = ex;

        if (!HasResponseStarted)
        {
            SetErrorResponseHeaders(ex.StatusCode);
        }

        DisableKeepAlive(HttpConnection1.GetConnectionEndReason(ex));
    }

    public void InitializeBodyControl(MessageBody messageBody)
    {
        //todo
        //if (_bodyControl == null)
        //{
        //    _bodyControl = new BodyControl(bodyControl: this, this);
        //}

        //(RequestBody, ResponseBody, RequestBodyPipeReader, ResponseBodyPipeWriter) = _bodyControl.Start(messageBody);
        //_requestStreamInternal = RequestBody;
        //_responseStreamInternal = ResponseBody;
    }

    private void SetErrorResponseHeaders(int statusCode)
    {
        //todo
        //Debug.Assert(!HasResponseStarted, $"{nameof(SetErrorResponseHeaders)} called after response had already started.");

        //StatusCode = statusCode;
        //ReasonPhrase = null;

        //var responseHeaders = HttpResponseHeaders;
        //responseHeaders.Reset();

        //responseHeaders.ContentLength = 0;
    }
}