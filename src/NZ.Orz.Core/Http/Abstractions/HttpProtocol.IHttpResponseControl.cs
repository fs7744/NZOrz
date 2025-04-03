using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZ.Orz.Http;

public abstract partial class HttpProtocol : IHttpResponseControl
{
    public void Initialize(HttpConnectionContext context)
    {
        Reset();

        HttpResponseControl = this;
    }

    public IHttpResponseControl HttpResponseControl { get; set; } = default!;

    public long UnflushedBytes => throw new NotImplementedException();

    public void Advance(int bytes)
    {
        throw new NotImplementedException();
    }

    public void CancelPendingFlush()
    {
        throw new NotImplementedException();
    }

    public Task CompleteAsync(Exception? exception = null)
    {
        throw new NotImplementedException();
    }

    public ValueTask<FlushResult> FlushPipeAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        throw new NotImplementedException();
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        throw new NotImplementedException();
    }

    public ValueTask<FlushResult> ProduceContinueAsync()
    {
        throw new NotImplementedException();
    }

    public ValueTask<FlushResult> WritePipeAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}