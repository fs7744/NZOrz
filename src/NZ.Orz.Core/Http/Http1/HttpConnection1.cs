using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Connections.Exceptions;
using NZ.Orz.Http.Exceptions;
using NZ.Orz.Infrastructure;
using NZ.Orz.Servers;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NZ.Orz.Http;

public class HttpConnection1 : HttpProtocol
{
    private const byte ByteCR = (byte)'\r';
    private const byte ByteLF = (byte)'\n';
    private const byte ByteAsterisk = (byte)'*';
    private const byte ByteForwardSlash = (byte)'/';
    private const string Asterisk = "*";
    private const string ForwardSlash = "/";
    private const byte ByteColon = (byte)':';
    private const byte ByteSpace = (byte)' ';
    private const byte ByteTab = (byte)'\t';
    private const byte ByteQuestionMark = (byte)'?';
    private const byte BytePercentage = (byte)'%';
    private const int MinTlsRequestSize = 1;
    private static ReadOnlySpan<byte> RequestLineDelimiters => [ByteLF, 0];

    private readonly ServiceContext serviceContext;
    private ServerLimits limits;
    private long _remainingRequestHeadersBytesAllowed;
    private readonly bool _showErrorDetails;
    private readonly bool _disableHttp1LineFeedTerminators;
    private uint _requestCount;
    private volatile bool _requestTimedOut;
    private HttpRequestTarget _requestTargetForm = HttpRequestTarget.Unknown;
    private Uri? _absoluteRequestTarget;
    private string? _parsedPath;
    private string? _parsedQueryString;
    private string? _parsedRawTarget;
    private Uri? _parsedAbsoluteRequestTarget;
    private bool _http2PrefaceDetected;

    public HttpConnection1(ConnectionContext connectionContext, ServiceContext serviceContext, TimeoutControl timeoutControl) : base(GatewayProtocols.HTTP1, connectionContext)
    {
        Input = connectionContext.Transport.Input;
        this.serviceContext = serviceContext;
        TimeoutControl = timeoutControl;
        this.limits = serviceContext.ServerOptions.Limits;
        _showErrorDetails = serviceContext.Log.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information);
        _disableHttp1LineFeedTerminators = serviceContext.ServerOptions.DisableHttp1LineFeedTerminators;
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
            var result = ParseHeaders(trailers, ref reader);
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

    private void OnStartLine(HttpVersion httpVersion, HttpMethod method, int methodEnd, TargetOffsetPathLength targetPath, Span<byte> startLine)
    {
        //todo check
        var targetStart = targetPath.Offset;
        // Slice out target
        var target = startLine[targetStart..];
        Debug.Assert(target.Length != 0, "Request target must be non-zero length");
        var ch = target[0];
        if (ch == ByteForwardSlash)
        {
            // origin-form.
            // The most common form of request-target.
            // https://tools.ietf.org/html/rfc7230#section-5.3.1
            OnOriginFormTarget(targetPath, target);
        }
        else if (ch == ByteAsterisk && target.Length == 1)
        {
            OnAsteriskFormTarget(method);
        }
        else if (startLine[targetStart..].GetKnownHttpScheme(out _))
        {
            OnAbsoluteFormTarget(targetPath, target);
        }
        else
        {
            // Assume anything else is considered authority form.
            // FYI: this should be an edge case. This should only happen when
            // a client mistakenly thinks this server is a proxy server.
            OnAuthorityFormTarget(method, target);
        }

        Method = method;
        if (method == HttpMethod.Custom)
        {
            _methodText = startLine[..methodEnd].GetAsciiString();
        }
        Version = httpVersion;
    }

    private void OnAuthorityFormTarget(HttpMethod method, Span<byte> target)
    {
        _requestTargetForm = HttpRequestTarget.AuthorityForm;

        // This is not complete validation. It is just a quick scan for invalid characters
        // but doesn't check that the target fully matches the URI spec.
        if (HttpUtilities.ContainsInvalidAuthorityChar(target))
        {
            ThrowRequestTargetRejected(target);
        }

        // The authority-form of request-target is only used for CONNECT
        // requests (https://tools.ietf.org/html/rfc9110#section-9.3.6).
        if (method != HttpMethod.Connect)
        {
            throw BadHttpRequestException.GetException(RequestRejectionReason.ConnectMethodRequired);
        }

        // When making a CONNECT request to establish a tunnel through one or
        // more proxies, a client MUST send only the target URI's authority
        // component (excluding any userinfo and its "@" delimiter) as the
        // request-target.For example,
        //
        //  CONNECT www.example.com:80 HTTP/1.1
        //
        // Allowed characters in the 'host + port' section of authority.
        // See https://tools.ietf.org/html/rfc3986#section-3.2

        var previousValue = _parsedRawTarget;
        if (previousValue == null || previousValue.Length != target.Length ||
            !StringUtilities.BytesOrdinalEqualsStringAndAscii(previousValue, target))
        {
            // The previous string does not match what the bytes would convert to,
            // so we will need to generate a new string.
            RawTarget = _parsedRawTarget = target.GetAsciiString();
        }
        else
        {
            // Reuse previous value
            RawTarget = _parsedRawTarget;
        }

        Path = string.Empty;
        QueryString = string.Empty;
        // Clear parsedData for path, queryString and absolute target as we won't check it if we come via this path again,
        // an setting to null is fast as it doesn't need to use a GC write barrier.
        _parsedPath = _parsedQueryString = null;
        _parsedAbsoluteRequestTarget = null;
    }

    private void OnAbsoluteFormTarget(TargetOffsetPathLength targetPath, Span<byte> target)
    {
        Span<byte> query = target[targetPath.Length..];
        _requestTargetForm = HttpRequestTarget.AbsoluteForm;

        // absolute-form
        // https://tools.ietf.org/html/rfc7230#section-5.3.2

        // This code should be the edge-case.

        // From the spec:
        //    a server MUST accept the absolute-form in requests, even though
        //    HTTP/1.1 clients will only send them in requests to proxies.

        var previousValue = _parsedRawTarget;
        if (previousValue == null || previousValue.Length != target.Length ||
            !StringUtilities.BytesOrdinalEqualsStringAndAscii(previousValue, target))
        {
            try
            {
                // The previous string does not match what the bytes would convert to,
                // so we will need to generate a new string.
                RawTarget = _parsedRawTarget = target.GetAsciiString();
            }
            catch (InvalidOperationException)
            {
                // GetAsciiStringNonNullCharacters throws an InvalidOperationException if there are
                // invalid characters in the string. This is hard to understand/diagnose, so let's
                // catch it and instead throw a more meaningful error. This matches the behavior in
                // the origin-form case.
                ThrowRequestTargetRejected(target);
            }

            // Validation of absolute URIs is slow, but clients
            // should not be sending this form anyways, so perf optimization
            // not high priority

            if (!Uri.TryCreate(RawTarget, UriKind.Absolute, out var uri))
            {
                ThrowRequestTargetRejected(target);
            }

            _absoluteRequestTarget = _parsedAbsoluteRequestTarget = uri;
            Path = _parsedPath = uri.LocalPath;
            // don't use uri.Query because we need the unescaped version
            previousValue = _parsedQueryString;
            if (previousValue == null || previousValue.Length != query.Length ||
                !StringUtilities.BytesOrdinalEqualsStringAndAscii(previousValue, query))
            {
                // The previous string does not match what the bytes would convert to,
                // so we will need to generate a new string.
                QueryString = _parsedQueryString = query.GetAsciiString();
            }
            else
            {
                QueryString = _parsedQueryString;
            }
        }
        else
        {
            // As RawTarget is the same we can reuse the previous values.
            RawTarget = _parsedRawTarget;
            Path = _parsedPath;
            QueryString = _parsedQueryString;
            _absoluteRequestTarget = _parsedAbsoluteRequestTarget;
        }
    }

    private void OnAsteriskFormTarget(HttpMethod method)
    {
        _requestTargetForm = HttpRequestTarget.AsteriskForm;

        // The asterisk-form of request-target is only used for a server-wide
        // OPTIONS request (https://tools.ietf.org/html/rfc9110#section-9.3.7).
        if (method != HttpMethod.Options)
        {
            throw BadHttpRequestException.GetException(RequestRejectionReason.OptionsMethodRequired);
        }

        RawTarget = Asterisk;
        Path = string.Empty;
        QueryString = string.Empty;
        // Clear parsedData as we won't check it if we come via this path again,
        // an setting to null is fast as it doesn't need to use a GC write barrier.
        _parsedRawTarget = _parsedPath = _parsedQueryString = null;
        _parsedAbsoluteRequestTarget = null;
    }

    // Compare with Http2Stream.TryValidatePseudoHeaders
    private void OnOriginFormTarget(TargetOffsetPathLength targetPath, Span<byte> target)
    {
        Debug.Assert(target[0] == ByteForwardSlash, "Should only be called when path starts with /");

        _requestTargetForm = HttpRequestTarget.OriginForm;

        if (target.Length == 1)
        {
            // If target.Length == 1 it can only be a forward slash (e.g. home page)
            // and we know RawTarget and Path are the same and QueryString is Empty
            RawTarget = ForwardSlash;
            Path = ForwardSlash;
            QueryString = string.Empty;
            // Clear parsedData as we won't check it if we come via this path again,
            // an setting to null is fast as it doesn't need to use a GC write barrier.
            _parsedRawTarget = _parsedPath = _parsedQueryString = null;
            _parsedAbsoluteRequestTarget = null;
            return;
        }

        // Read raw target before mutating memory.
        var previousValue = _parsedRawTarget;
        if (previousValue == null || previousValue.Length != target.Length ||
            !StringUtilities.BytesOrdinalEqualsStringAndAscii(previousValue, target))
        {
            ParseTarget(targetPath, target);
        }
        else
        {
            // As RawTarget is the same we can reuse the previous parsed values.
            RawTarget = previousValue;
            Path = _parsedPath;
            QueryString = _parsedQueryString;
        }

        // Clear parsedData for absolute target as we won't check it if we come via this path again,
        // an setting to null is fast as it doesn't need to use a GC write barrier.
        _parsedAbsoluteRequestTarget = null;
    }

    private void ParseTarget(TargetOffsetPathLength targetPath, Span<byte> target)
    {
        // URIs are always encoded/escaped to ASCII https://tools.ietf.org/html/rfc3986#page-11
        // Multibyte Internationalized Resource Identifiers (IRIs) are first converted to utf8;
        // then encoded/escaped to ASCII  https://www.ietf.org/rfc/rfc3987.txt "Mapping of IRIs to URIs"

        try
        {
            // The previous string does not match what the bytes would convert to,
            // so we will need to generate a new string.
            RawTarget = _parsedRawTarget = target.GetAsciiString();

            var queryLength = 0;
            if (target.Length == targetPath.Length)
            {
                // No query string
                if (ReferenceEquals(_parsedQueryString, string.Empty))
                {
                    QueryString = _parsedQueryString;
                }
                else
                {
                    QueryString = string.Empty;
                    _parsedQueryString = string.Empty;
                }
            }
            else
            {
                queryLength = ParseQuery(targetPath, target);
            }

            var pathLength = targetPath.Length;
            if (pathLength == 1)
            {
                // If path.Length == 1 it can only be a forward slash (e.g. home page)
                Path = _parsedPath = ForwardSlash;
            }
            else
            {
                var path = target[..pathLength];
                Path = _parsedPath = HttpUtilities.DecodePath(path, targetPath.IsEncoded, RawTarget, queryLength);
            }
        }
        catch (InvalidOperationException)
        {
            ThrowRequestTargetRejected(target);
        }
    }

    private int ParseQuery(TargetOffsetPathLength targetPath, Span<byte> target)
    {
        var previousValue = _parsedQueryString;
        var query = target[targetPath.Length..];
        var queryLength = query.Length;
        if (previousValue == null || previousValue.Length != queryLength ||
            !StringUtilities.BytesOrdinalEqualsStringAndAscii(previousValue, query))
        {
            // The previous string does not match what the bytes would convert to,
            // so we will need to generate a new string.
            QueryString = _parsedQueryString = query.GetAsciiString();
        }
        else
        {
            // Same as previous
            QueryString = _parsedQueryString;
        }

        return queryLength;
    }

    [StackTraceHidden]
    [DoesNotReturn]
    public void ThrowRequestTargetRejected(Span<byte> target)
    => throw GetInvalidRequestTargetException(target);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private BadHttpRequestException GetInvalidRequestTargetException(ReadOnlySpan<byte> target)
        => BadHttpRequestException.GetException(
            RequestRejectionReason.InvalidRequestTarget,
            _showErrorDetails
                ? target.GetAsciiStringEscaped(MaxExceptionDetailSize)
                : string.Empty);

    #region HttpParser

    public bool ParseRequestLine(ref SequenceReader<byte> reader)
    {
        // Find the next delimiter.
        if (!reader.TryReadToAny(out ReadOnlySpan<byte> requestLine, RequestLineDelimiters, advancePastDelimiter: false))
        {
            return false;
        }
        // Consume the delimiter.
        reader.TryRead(out var next);
        // If null character found, or request line is empty
        if (next == 0 || requestLine.Length == 0)
        {
            // Rewind and re-read to format error message correctly
            reader.Rewind(requestLine.Length + 1);
            reader.TryReadExact(requestLine.Length + 1, out var requestLineSequence);
            requestLine = requestLineSequence.IsSingleSegment ? requestLineSequence.FirstSpan : requestLineSequence.ToArray();
            RejectRequestLine(requestLine);
        }

        // Get Method and set the offset
        var method = requestLine.GetKnownMethod(out var methodEnd);
        if (method == HttpMethod.Custom)
        {
            methodEnd = GetUnknownMethodLength(requestLine);
        }

        // Use a new offset var as methodEnd needs to be on stack
        // as its passed by reference above so can't be in register.
        // Skip space
        var offset = methodEnd + 1;
        if ((uint)offset >= (uint)requestLine.Length)
        {
            // Start of path not found
            RejectRequestLine(requestLine);
        }

        var ch = requestLine[offset];
        if (ch == ByteSpace || ch == ByteQuestionMark || ch == BytePercentage)
        {
            // Empty path is illegal, or path starting with percentage
            RejectRequestLine(requestLine);
        }

        // Target = Path and Query
        var targetStart = offset;
        var pathEncoded = false;
        // Skip first char (just checked)
        offset++;

        // Find end of path and if path is encoded
        var index = requestLine.Slice(offset).IndexOfAny(ByteSpace, ByteQuestionMark, BytePercentage);
        if (index >= 0)
        {
            if (requestLine[offset + index] == BytePercentage)
            {
                pathEncoded = true;
                offset += index;
                // Found an encoded character, now just search for end of path
                index = requestLine.Slice(offset).IndexOfAny(ByteSpace, ByteQuestionMark);
            }

            offset += index;
            ch = requestLine[offset];
        }

        var path = new TargetOffsetPathLength(targetStart, length: offset - targetStart, pathEncoded);

        // Query string
        if (ch == ByteQuestionMark)
        {
            // We have a query string
            for (; (uint)offset < (uint)requestLine.Length; offset++)
            {
                ch = requestLine[offset];
                if (ch == ByteSpace)
                {
                    break;
                }
            }
        }

        var queryEnd = offset;
        // Consume space
        offset++;

        while ((uint)offset < (uint)requestLine.Length
            && requestLine[offset] == ByteSpace)
        {
            // It's invalid to have multiple spaces between the url resource and version
            // but some clients do it. Skip them.
            offset++;
        }

        // Version + CR is 9 bytes which should take us to .Length
        // LF should have been dropped prior to method call
        if ((uint)offset + 9 != (uint)requestLine.Length || requestLine[offset + 8] != ByteCR)
        {
            // LF should have been dropped prior to method call
            // If !_disableHttp1LineFeedTerminators and offset + 8 is .Length,
            // then requestLine is valid since it means LF was the next char
            if (_disableHttp1LineFeedTerminators || (uint)offset + 8 != (uint)requestLine.Length)
            {
                RejectRequestLine(requestLine);
            }
        }

        // Version
        var remaining = requestLine.Slice(offset);
        var httpVersion = remaining.GetKnownVersion();
        if (httpVersion == HttpVersion.Unknown)
        {
            // HTTP version is unsupported.
            RejectUnknownVersion(remaining);
        }

        // We need to reinterpret from ReadOnlySpan into Span to allow path mutation for
        // in-place normalization and decoding to transform into a canonical path
        var startLine = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(requestLine), queryEnd);
        OnStartLine(httpVersion, method, methodEnd, path, startLine);

        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private int GetUnknownMethodLength(ReadOnlySpan<byte> span)
    {
        var invalidIndex = HttpUtilities.IndexOfInvalidTokenChar(span);

        if (invalidIndex <= 0 || span[invalidIndex] != ByteSpace)
        {
            RejectRequestLine(span);
        }

        return invalidIndex;
    }

    private static bool IsTlsHandshake(ReadOnlySpan<byte> requestLine)
    {
        const byte SslRecordTypeHandshake = (byte)0x16;

        // Make sure we can check at least for the existence of a TLS handshake - we check the first byte
        // See https://serializethoughts.com/2014/07/27/dissecting-tls-client-hello-message/

        return (requestLine.Length >= MinTlsRequestSize && requestLine[0] == SslRecordTypeHandshake);
    }

    [StackTraceHidden]
    private void RejectRequestLine(ReadOnlySpan<byte> requestLine)
    {
        throw GetInvalidRequestException(
            IsTlsHandshake(requestLine) ?
            RequestRejectionReason.TlsOverHttpError :
            RequestRejectionReason.InvalidRequestLine,
            requestLine);
    }

    [StackTraceHidden]
    private void RejectRequestHeader(ReadOnlySpan<byte> headerLine)
        => throw GetInvalidRequestException(RequestRejectionReason.InvalidRequestHeader, headerLine);

    [StackTraceHidden]
    private void RejectUnknownVersion(ReadOnlySpan<byte> version)
        => throw GetInvalidRequestException(RequestRejectionReason.UnrecognizedHTTPVersion, version[..^1]);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private BadHttpRequestException GetInvalidRequestException(RequestRejectionReason reason, ReadOnlySpan<byte> headerLine)
        => BadHttpRequestException.GetException(
            reason,
            _showErrorDetails
                ? headerLine.GetAsciiStringEscaped(MaxExceptionDetailSize)
                : string.Empty);

    private bool ParseHeaders(bool trailers, ref SequenceReader<byte> reader)
    {
        throw new NotImplementedException();
    }

    #endregion HttpParser
}