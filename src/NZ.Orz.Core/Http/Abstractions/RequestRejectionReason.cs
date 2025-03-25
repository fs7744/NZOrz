namespace NZ.Orz.Http;

public enum RequestRejectionReason
{
    Unknown,
    TlsOverHttpError,
    UnrecognizedHTTPVersion,
    InvalidRequestLine,
    InvalidRequestHeader,
    InvalidRequestHeadersNoCRLF,
    MalformedRequestInvalidHeaders,
    InvalidContentLength,
    MultipleContentLengths,
    UnexpectedEndOfRequestContent,
    BadChunkSuffix,
    BadChunkSizeData,
    ChunkedRequestIncomplete,
    InvalidRequestTarget,
    InvalidCharactersInHeaderName,
    RequestLineTooLong,
    HeadersExceedMaxTotalSize,
    TooManyHeaders,
    RequestBodyTooLarge,
    RequestHeadersTimeout,
    RequestBodyTimeout,
    FinalTransferCodingNotChunked,
    LengthRequiredHttp10,
    OptionsMethodRequired,
    ConnectMethodRequired,
    MissingHostHeader,
    MultipleHostHeaders,
    InvalidHostHeader
}