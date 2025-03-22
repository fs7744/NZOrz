using NZ.Orz.Connections;
using NZ.Orz.Connections.Exceptions;

namespace NZ.Orz.Http;

internal interface IRequestProcessor
{
    Task ProcessRequestsAsync(HttpConnectionDelegate application);

    void StopProcessingNextRequest(ConnectionEndReason reason);

    void HandleRequestHeadersTimeout();

    void HandleReadDataRateTimeout();

    void OnInputOrOutputCompleted();

    void Tick(long timestamp);

    void Abort(ConnectionAbortedException ex, ConnectionEndReason reason);
}