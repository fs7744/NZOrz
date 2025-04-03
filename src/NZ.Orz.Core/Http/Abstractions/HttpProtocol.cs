using Microsoft.Extensions.Logging;
using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Connections.Exceptions;
using NZ.Orz.Http.Exceptions;
using NZ.Orz.Metrics;
using NZ.Orz.Servers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Net;

namespace NZ.Orz.Http;

public abstract partial class HttpProtocol : HttpConnectionContext, IRequestProcessor
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
    private bool _isLeasedMemoryInvalid = true;
    private bool _canWriteResponseBody = true;
    private bool _hasAdvanced;
    private long _responseBytesWritten;

    private int _statusCode;

    public int StatusCode
    {
        get => _statusCode;
        set
        {
            if (HasResponseStarted)
            {
                ThrowResponseAlreadyStartedException(nameof(StatusCode));
            }

            _statusCode = value;
        }
    }

    private string? _reasonPhrase;

    public string? ReasonPhrase
    {
        get => _reasonPhrase;

        set
        {
            if (HasResponseStarted)
            {
                ThrowResponseAlreadyStartedException(nameof(ReasonPhrase));
            }

            _reasonPhrase = value;
        }
    }

    public CancellationToken RequestAborted
    {
        get
        {
            // If a request abort token was previously explicitly set, return it.
            if (_manuallySetRequestAbortToken.HasValue)
            {
                return _manuallySetRequestAbortToken.Value;
            }

            lock (_abortLock)
            {
                if (_preventRequestAbortedCancellation)
                {
                    return new CancellationToken(false);
                }

                if (_connectionAborted && _abortedCts == null)
                {
                    return new CancellationToken(true);
                }

                if (_abortedCts == null)
                {
                    _abortedCts = new CancellationTokenSource();
                }

                return _abortedCts.Token;
            }
        }
        set
        {
            // Set an abort token, overriding one we create internally.  This setter and associated
            // field exist purely to support IHttpRequestLifetimeFeature.set_RequestAborted.
            _manuallySetRequestAbortToken = value;
        }
    }

    public IHttpOutputProducer Output { get; protected set; } = default!;
    protected BodyControl? _bodyControl;
    private Stack<KeyValuePair<Func<object, Task>, object>>? _onStarting;
    private Stack<KeyValuePair<Func<object, Task>, object>>? _onCompleted;
#if NET9_0_OR_GREATER
    private readonly Lock _abortLock = new();
#else
    private readonly object _abortLock = new();
#endif
    protected volatile bool _connectionAborted;
    private bool _preventRequestAbortedCancellation;
    private CancellationTokenSource? _abortedCts;
    private CancellationToken? _manuallySetRequestAbortToken;

    public HttpMethod Method { get; set; }
    public HttpVersion Version { get; set; }

    public bool IsUpgradableRequest { get; private set; }
    public bool IsUpgraded { get; set; }
    public bool IsExtendedConnectRequest { get; set; }
    public bool IsExtendedConnectAccepted { get; set; }
    private IPEndPoint? LocalEndPoint => ConnectionContext.LocalEndPoint as IPEndPoint;
    private IPEndPoint? RemoteEndPoint => ConnectionContext.RemoteEndPoint as IPEndPoint;
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
        try
        {
            await DoProcessRequestsAsync(application);
        }
        catch (BadHttpRequestException ex)
        {
            // Handle BadHttpRequestException thrown during request line or header parsing.
            // SetBadRequestState logs the error.
            SetBadRequestState(ex);
        }
        catch (IOException ex)
        {
            Log.RequestProcessingError(ConnectionId, ex);
        }
        catch (ConnectionAbortedException ex)
        {
            Log.RequestProcessingError(ConnectionId, ex);
        }
        catch (Exception ex)
        {
            Log.LogWarning(0, ex, "Connection processing ended abnormally.");
        }
        finally
        {
            try
            {
                if (_requestRejectedException != null)
                {
                    await TryProduceInvalidRequestResponse();
                }
            }
            catch (Exception ex)
            {
                Log.LogWarning(0, ex, "Connection shutdown abnormally.");
            }
            finally
            {
                OnRequestProcessingEnded();
            }
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
            IsUpgradableRequest = messageBody.RequestUpgrade;

            InitializeBodyControl(messageBody);

            try
            {
                // Run the application code for this request
                await application(this);

                // Trigger OnStarting if it hasn't been called yet and the app hasn't
                // already failed. If an OnStarting callback throws we can go through
                // our normal error handling in ProduceEnd.
                // https://github.com/aspnet/KestrelHttpServer/issues/43
                if (!HasResponseStarted && _applicationException == null && _onStarting?.Count > 0)
                {
                    await FireOnStarting();
                }

                if (!_connectionAborted && !VerifyResponseContentLength(out var lengthException))
                {
                    ReportApplicationError(lengthException);
                }
            }
            catch (BadHttpRequestException ex)
            {
                // Capture BadHttpRequestException for further processing
                // This has to be caught here so StatusCode is set properly before disposing the HttpContext
                // (DisposeContext logs StatusCode).
                SetBadRequestState(ex);
                ReportApplicationError(ex);
            }
            catch (Exception ex)
            {
                if ((ex is OperationCanceledException || ex is IOException) && _connectionAborted)
                {
                    Log.RequestAborted(ConnectionId, TraceIdentifier);
                }
                else
                {
                    ReportApplicationError(ex);
                }
            }

            // At this point all user code that needs use to the request or response streams has completed.
            // Using these streams in the OnCompleted callback is not allowed.
            try
            {
                Debug.Assert(_bodyControl != null);
                await _bodyControl.StopAsync();
            }
            catch (Exception ex)
            {
                // BodyControl.StopAsync() can throw if the PipeWriter was completed prior to the application writing
                // enough bytes to satisfy the specified Content-Length. This risks double-logging the exception,
                // but this scenario generally indicates an app bug, so I don't want to risk not logging it.
                ReportApplicationError(ex);
            }

            // 4XX responses are written by TryProduceInvalidRequestResponse during connection tear down.
            if (_requestRejectedException == null)
            {
                if (!_connectionAborted)
                {
                    // Call ProduceEnd() before consuming the rest of the request body to prevent
                    // delaying clients waiting for the chunk terminator:
                    //
                    // https://github.com/dotnet/corefx/issues/17330#issuecomment-288248663
                    //
                    // This also prevents the 100 Continue response from being sent if the app
                    // never tried to read the body.
                    // https://github.com/aspnet/KestrelHttpServer/issues/2102
                    //
                    // ProduceEnd() must be called before _application.DisposeContext(), to ensure
                    // HttpContext.Response.StatusCode is correctly set when
                    // IHttpContextFactory.Dispose(HttpContext) is called.
                    await ProduceEnd();
                }
                else if (!HasResponseStarted)
                {
                    // If the request was aborted and no response was sent, we use status code 499 for logging
                    StatusCode = StatusCodes.Status499ClientClosedRequest;
                }
            }

            if (_onCompleted?.Count > 0)
            {
                await FireOnCompleted();
            }

            // Even for non-keep-alive requests, try to consume the entire body to avoid RSTs.
            if (!_connectionAborted && _requestRejectedException == null && !messageBody.IsEmpty)
            {
                await messageBody.ConsumeAsync();
            }

            if (HasStartedConsumingRequestBody)
            {
                await messageBody.StopAsync();
            }
        }
    }

    protected Task FireOnStarting()
    {
        var onStarting = _onStarting;
        if (onStarting?.Count > 0)
        {
            return ProcessEvents(this, onStarting);
        }

        return Task.CompletedTask;

        static async Task ProcessEvents(HttpProtocol protocol, Stack<KeyValuePair<Func<object, Task>, object>> events)
        {
            // Try/Catch is outside the loop as any error that occurs is before the request starts.
            // So we want to report it as an ApplicationError to fail the request and not process more events.
            try
            {
                while (events.TryPop(out var entry))
                {
                    await entry.Key.Invoke(entry.Value);
                }
            }
            catch (Exception ex)
            {
                protocol.ReportApplicationError(ex);
            }
        }
    }

    protected Task FireOnCompleted()
    {
        var onCompleted = _onCompleted;
        if (onCompleted?.Count > 0)
        {
            return ProcessEvents(this, onCompleted);
        }

        return Task.CompletedTask;

        static async Task ProcessEvents(HttpProtocol protocol, Stack<KeyValuePair<Func<object, Task>, object>> events)
        {
            // Try/Catch is inside the loop as any error that occurs is after the request has finished.
            // So we will just log it and keep processing the events, as the completion has already happened.
            while (events.TryPop(out var entry))
            {
                try
                {
                    await entry.Key.Invoke(entry.Value);
                }
                catch (Exception ex)
                {
                    protocol.Log.ApplicationError(protocol.ConnectionId, protocol.TraceIdentifier, ex);
                }
            }
        }
    }

    protected bool VerifyResponseContentLength([NotNullWhen(false)] out Exception? ex)
    {
        // TODO
        //var responseHeaders = HttpResponseHeaders;

        //if (Method != HttpMethod.Head &&
        //    StatusCode != StatusCodes.Status304NotModified &&
        //    !responseHeaders.HasTransferEncoding &&
        //    responseHeaders.ContentLength.HasValue &&
        //    _responseBytesWritten < responseHeaders.ContentLength.Value)
        //{
        //    // We need to close the connection if any bytes were written since the client
        //    // cannot be certain of how many bytes it will receive.
        //    if (_responseBytesWritten > 0)
        //    {
        //        DisableKeepAlive(ConnectionEndReason.ResponseContentLengthMismatch);
        //    }

        //    ex = new InvalidOperationException(
        //        CoreStrings.FormatTooFewBytesWritten(_responseBytesWritten, responseHeaders.ContentLength.Value));
        //    return false;
        //}

        ex = null;
        return true;
    }

    public void Reset()
    {
        _onStarting?.Clear();
        _onCompleted?.Clear();
        //_routeValues?.Clear();

        _requestProcessingStatus = RequestProcessingStatus.RequestPending;
        //_autoChunk = false;
        _applicationException = null;
        _requestRejectedException = null;

        HasStartedConsumingRequestBody = false;
        MaxRequestBodySize = ServiceContext.ServerOptions.Limits.MaxRequestBodySize;
        MinRequestBodyDataRate = ServiceContext.ServerOptions.Limits.MinRequestBodyDataRate;
        TraceIdentifier = null!;
        Method = HttpMethod.None;
        _methodText = null;
        //_endpoint = null;
        PathBase = null;
        Path = null;
        RawTarget = null;
        QueryString = null;
        Version = Http.HttpVersion.Unknown;
        //_httpProtocol = null;
        _statusCode = StatusCodes.Status200OK;
        _reasonPhrase = null;
        IsUpgraded = false;
        IsExtendedConnectRequest = false;
        IsExtendedConnectAccepted = false;
        //IsWebTransportRequest = false;
        ConnectProtocol = null;

        var remoteEndPoint = RemoteEndPoint;
        RemoteIpAddress = remoteEndPoint?.Address;
        RemotePort = remoteEndPoint?.Port ?? 0;
        var localEndPoint = LocalEndPoint;
        LocalIpAddress = localEndPoint?.Address;
        LocalPort = localEndPoint?.Port ?? 0;

        RequestHeaders.Clear();
        //HttpRequestHeaders.EncodingSelector = ServerOptions.RequestHeaderEncodingSelector;
        //HttpRequestHeaders.ReuseHeaderValues = !ServerOptions.DisableStringReuse;
        //HttpResponseHeaders.Reset();
        //HttpResponseHeaders.EncodingSelector = ServerOptions.ResponseHeaderEncodingSelector;
        //RequestHeaders = HttpRequestHeaders;
        //ResponseHeaders = HttpResponseHeaders;
        //RequestTrailers.Clear();
        //ResponseTrailers?.Reset();
        //RequestTrailersAvailable = false;

        _isLeasedMemoryInvalid = true;
        _hasAdvanced = false;
        _canWriteResponseBody = true;

        //if (_scheme == null)
        //{
        //    var tlsFeature = ConnectionFeatures?[typeof(ITlsConnectionFeature)];
        //    _scheme = tlsFeature != null ? SchemeHttps : SchemeHttp;
        //}

        //Scheme = _scheme;

        _manuallySetRequestAbortToken = null;

        // Lock to prevent CancelRequestAbortedToken from attempting to cancel a disposed CTS.
        CancellationTokenSource? localAbortCts = null;

        lock (_abortLock)
        {
            _preventRequestAbortedCancellation = false;
            if (_abortedCts?.TryReset() == false)
            {
                localAbortCts = _abortedCts;
                _abortedCts = null;
            }
        }

        localAbortCts?.Dispose();

        //Output?.Reset();

        _requestHeadersParsed = 0;
        //_eagerRequestHeadersParsedLimit = ServerOptions.Limits.MaxRequestHeaderCount;

        _responseBytesWritten = 0;

        OnReset();
    }

    protected virtual Task TryProduceInvalidRequestResponse()
    {
        Debug.Assert(_requestRejectedException != null);

        // If _connectionAborted is set, the connection has already been closed.
        if (!_connectionAborted)
        {
            return ProduceEnd();
        }

        return Task.CompletedTask;
    }

    protected Task ProduceEnd()
    {
        if (HasResponseCompleted)
        {
            return Task.CompletedTask;
        }

        _isLeasedMemoryInvalid = true;

        if (_requestRejectedException != null || _applicationException != null)
        {
            if (HasResponseStarted)
            {
                // We can no longer change the response, so we simply close the connection.
                DisableKeepAlive(ConnectionEndReason.ErrorAfterStartingResponse);
                OnErrorAfterResponseStarted();
                return Task.CompletedTask;
            }

            // If the request was rejected, the error state has already been set by SetBadRequestState and
            // that should take precedence.
            if (_requestRejectedException != null)
            {
                SetErrorResponseHeaders(_requestRejectedException.StatusCode);
            }
            else
            {
                // 500 Internal Server Error
                SetErrorResponseHeaders(statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        if (!HasResponseStarted)
        {
            ProduceStart(appCompleted: true);
        }

        return WriteSuffix();
    }

    private void ProduceStart(bool appCompleted)
    {
        if (HasResponseStarted)
        {
            return;
        }

        _isLeasedMemoryInvalid = true;

        _requestProcessingStatus = RequestProcessingStatus.HeadersCommitted;

        var responseHeaders = CreateResponseHeaders(appCompleted);

        Output.WriteResponseHeaders(StatusCode, ReasonPhrase, responseHeaders, //_autoChunk,
                                                                               appCompleted);
    }

    private Task WriteSuffix()
    {
        if (
            //_autoChunk ||
            Version >= Http.HttpVersion.Http2)
        {
            // For the same reason we call CheckLastWrite() in Content-Length responses.
            PreventRequestAbortedCancellation();
        }

        var writeTask = Output.WriteStreamSuffixAsync();

        if (!writeTask.IsCompletedSuccessfully)
        {
            return WriteSuffixAwaited(writeTask);
        }

        writeTask.GetAwaiter().GetResult();

        _requestProcessingStatus = RequestProcessingStatus.ResponseCompleted;

        if (_keepAlive)
        {
            Log.ConnectionKeepAlive(ConnectionId);
        }

        if (Method == HttpMethod.Head && _responseBytesWritten > 0)
        {
            Log.ConnectionHeadResponseBodyWrite(ConnectionId, _responseBytesWritten);
        }

        return Task.CompletedTask;
    }

    private async Task WriteSuffixAwaited(ValueTask<FlushResult> writeTask)
    {
        _requestProcessingStatus = RequestProcessingStatus.HeadersFlushed;

        await writeTask;

        _requestProcessingStatus = RequestProcessingStatus.ResponseCompleted;

        if (_keepAlive)
        {
            Log.ConnectionKeepAlive(ConnectionId);
        }

        if (Method == HttpMethod.Head && _responseBytesWritten > 0)
        {
            Log.ConnectionHeadResponseBodyWrite(ConnectionId, _responseBytesWritten);
        }
    }

    // Prevents the RequestAborted token from firing for the duration of the request.
    private void PreventRequestAbortedCancellation()
    {
        lock (_abortLock)
        {
            if (_connectionAborted)
            {
                return;
            }

            _preventRequestAbortedCancellation = true;
        }
    }

    protected abstract void OnErrorAfterResponseStarted();

    public abstract void OnRequestProcessingEnded();

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
        if (_bodyControl == null)
        {
            _bodyControl = new BodyControl(this);
        }

        (RequestBody, ResponseBody, RequestBodyPipeReader, ResponseBodyPipeWriter) = _bodyControl.Start(messageBody);
        _requestStreamInternal = RequestBody;
        _responseStreamInternal = ResponseBody;
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

    private static void ThrowResponseAlreadyStartedException(string value)
    {
        throw new InvalidOperationException($"{value} cannot be set because the response has already started.");
    }
}