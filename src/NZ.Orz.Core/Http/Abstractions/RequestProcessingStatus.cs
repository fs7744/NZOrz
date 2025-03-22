namespace NZ.Orz.Http;

public enum RequestProcessingStatus
{
    RequestPending,
    ParsingRequestLine,
    ParsingHeaders,
    AppStarted,
    HeadersCommitted,
    HeadersFlushed,
    ResponseCompleted
}