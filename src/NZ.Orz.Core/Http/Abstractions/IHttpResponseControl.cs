using System.IO.Pipelines;

namespace NZ.Orz.Http;

public interface IHttpResponseControl
{
    ValueTask<FlushResult> ProduceContinueAsync();

    Memory<byte> GetMemory(int sizeHint = 0);

    Span<byte> GetSpan(int sizeHint = 0);

    void Advance(int bytes);

    long UnflushedBytes { get; }

    ValueTask<FlushResult> FlushPipeAsync(CancellationToken cancellationToken);

    ValueTask<FlushResult> WritePipeAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken);

    void CancelPendingFlush();

    Task CompleteAsync(Exception? exception = null);
}