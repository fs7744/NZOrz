﻿using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NZ.Orz.Http.Exceptions;

public class BadHttpRequestException : IOException
{
    public BadHttpRequestException(string message, int statusCode, RequestRejectionReason reason)
        : this(message, statusCode)
    {
        Reason = reason;
    }

    public BadHttpRequestException(string message, RequestRejectionReason reason)
        : this(message)
    {
        Reason = reason;
    }

    public BadHttpRequestException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public BadHttpRequestException(string message)
        : base(message)
    {
        StatusCode = StatusCodes.Status400BadRequest;
    }

    public int StatusCode { get; }

    public RequestRejectionReason Reason { get; }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static BadHttpRequestException GetException(RequestRejectionReason reason)
    {
        BadHttpRequestException ex;
        switch (reason)
        {
            case RequestRejectionReason.InvalidRequestHeadersNoCRLF:
                ex = new BadHttpRequestException("Invalid request headers: missing final CRLF in header fields.", StatusCodes.Status400BadRequest, reason);
                break;

            case RequestRejectionReason.InvalidRequestLine:
                ex = new BadHttpRequestException("Invalid request line.", StatusCodes.Status400BadRequest, reason);
                break;

            case RequestRejectionReason.MalformedRequestInvalidHeaders:
                ex = new BadHttpRequestException("Malformed request: invalid headers.", StatusCodes.Status400BadRequest, reason);
                break;

            case RequestRejectionReason.MultipleContentLengths:
                ex = new BadHttpRequestException("Multiple Content-Length headers.", StatusCodes.Status400BadRequest, reason);
                break;

            case RequestRejectionReason.UnexpectedEndOfRequestContent:
                ex = new BadHttpRequestException("Unexpected end of request content.", StatusCodes.Status400BadRequest, reason);
                break;

            case RequestRejectionReason.BadChunkSuffix:
                ex = new BadHttpRequestException("Bad chunk suffix.", StatusCodes.Status400BadRequest, reason);
                break;

            case RequestRejectionReason.BadChunkSizeData:
                ex = new BadHttpRequestException("Bad chunk size data.", StatusCodes.Status400BadRequest, reason);
                break;

            case RequestRejectionReason.ChunkedRequestIncomplete:
                ex = new BadHttpRequestException("Chunked request incomplete.", StatusCodes.Status400BadRequest, reason);
                break;

            case RequestRejectionReason.InvalidCharactersInHeaderName:
                ex = new BadHttpRequestException("Invalid characters in header name.", StatusCodes.Status400BadRequest, reason);
                break;

            case RequestRejectionReason.RequestLineTooLong:
                ex = new BadHttpRequestException("Request line too long.", StatusCodes.Status414UriTooLong, reason);
                break;

            case RequestRejectionReason.HeadersExceedMaxTotalSize:
                ex = new BadHttpRequestException("Request headers too long.", StatusCodes.Status431RequestHeaderFieldsTooLarge, reason);
                break;

            case RequestRejectionReason.TooManyHeaders:
                ex = new BadHttpRequestException("Request contains too many headers.", StatusCodes.Status431RequestHeaderFieldsTooLarge, reason);
                break;

            case RequestRejectionReason.RequestHeadersTimeout:
                ex = new BadHttpRequestException("Reading the request headers timed out.", StatusCodes.Status408RequestTimeout, reason);
                break;

            case RequestRejectionReason.RequestBodyTimeout:
                ex = new BadHttpRequestException("Reading the request body timed out due to data arriving too slowly. See MinRequestBodyDataRate.", StatusCodes.Status408RequestTimeout, reason);
                break;

            case RequestRejectionReason.OptionsMethodRequired:
                ex = new BadHttpRequestException("Method not allowed.", StatusCodes.Status405MethodNotAllowed, reason);
                break;

            case RequestRejectionReason.ConnectMethodRequired:
                ex = new BadHttpRequestException("Method not allowed.", StatusCodes.Status405MethodNotAllowed, reason);
                break;

            case RequestRejectionReason.MissingHostHeader:
                ex = new BadHttpRequestException("Request is missing Host header.", StatusCodes.Status400BadRequest, reason);
                break;

            case RequestRejectionReason.MultipleHostHeaders:
                ex = new BadHttpRequestException("Multiple Host headers.", StatusCodes.Status400BadRequest, reason);
                break;

            case RequestRejectionReason.InvalidHostHeader:
                ex = new BadHttpRequestException("Invalid Host header.", StatusCodes.Status400BadRequest, reason);
                break;

            default:
                ex = new BadHttpRequestException("Bad request.", StatusCodes.Status400BadRequest, reason);
                break;
        }
        return ex;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static BadHttpRequestException GetException(RequestRejectionReason reason, string detail)
    {
        BadHttpRequestException ex;
        switch (reason)
        {
            case RequestRejectionReason.TlsOverHttpError:
                ex = new BadHttpRequestException("Detected a TLS handshake to an endpoint that does not have TLS enabled.", StatusCodes.Status400BadRequest, reason);
                break;

            case RequestRejectionReason.InvalidRequestLine:
                ex = new BadHttpRequestException($"Invalid request line: '{detail}'", StatusCodes.Status400BadRequest, reason);
                break;

            case RequestRejectionReason.InvalidRequestTarget:
                ex = new BadHttpRequestException($"Invalid request target: '{detail}'", StatusCodes.Status400BadRequest, reason);
                break;

            case RequestRejectionReason.InvalidRequestHeader:
                ex = new BadHttpRequestException($"Invalid request header: '{detail}'", StatusCodes.Status400BadRequest, reason);
                break;

            case RequestRejectionReason.InvalidContentLength:
                ex = new BadHttpRequestException($"Invalid content length: {detail}", StatusCodes.Status400BadRequest, reason);
                break;

            case RequestRejectionReason.UnrecognizedHTTPVersion:
                ex = new BadHttpRequestException($"Unrecognized HTTP version: '{detail}'", StatusCodes.Status505HttpVersionNotsupported, reason);
                break;

            case RequestRejectionReason.FinalTransferCodingNotChunked:
                ex = new BadHttpRequestException($"The message body length cannot be determined because the final transfer coding was set to '{detail}' instead of 'chunked'.", StatusCodes.Status400BadRequest, reason);
                break;

            case RequestRejectionReason.LengthRequiredHttp10:
                ex = new BadHttpRequestException($"{detail} request contains no Content-Length header.", StatusCodes.Status400BadRequest, reason);
                break;

            case RequestRejectionReason.InvalidHostHeader:
                ex = new BadHttpRequestException($"Invalid Host header: '{detail}'", StatusCodes.Status400BadRequest, reason);
                break;

            case RequestRejectionReason.RequestBodyTooLarge:
                ex = new BadHttpRequestException($"Request body too large. The max request body size is {detail} bytes.", StatusCodes.Status413PayloadTooLarge, reason);
                break;

            default:
                ex = new BadHttpRequestException("Bad request.", StatusCodes.Status400BadRequest, reason);
                break;
        }
        return ex;
    }

    internal static BadHttpRequestException GetException(RequestRejectionReason reason, HttpMethod method)
        => GetException(reason, method.ToString().ToUpperInvariant());
}