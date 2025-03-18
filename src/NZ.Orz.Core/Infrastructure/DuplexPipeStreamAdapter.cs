﻿using System.IO.Pipelines;

namespace NZ.Orz.Infrastructure;

public class DuplexPipeStreamAdapter<TStream> : DuplexPipeStream, IDuplexPipe where TStream : Stream
{
    private bool _disposed;
#if NET9_0_OR_GREATER
    private readonly Lock _disposeLock = new();
#else
    private readonly object _disposeLock = new();
#endif

    public DuplexPipeStreamAdapter(IDuplexPipe duplexPipe, Func<Stream, TStream> createStream) :
        this(duplexPipe, new StreamPipeReaderOptions(leaveOpen: true), new StreamPipeWriterOptions(leaveOpen: true), createStream)
    {
    }

    public DuplexPipeStreamAdapter(IDuplexPipe duplexPipe, StreamPipeReaderOptions readerOptions, StreamPipeWriterOptions writerOptions, Func<Stream, TStream> createStream) :
        base(duplexPipe.Input, duplexPipe.Output)
    {
        var stream = createStream(this);
        Stream = stream;
        Input = PipeReader.Create(stream, readerOptions);
        Output = PipeWriter.Create(stream, writerOptions);
    }

    public TStream Stream { get; }

    public PipeReader Input { get; }

    public PipeWriter Output { get; }

    public override async ValueTask DisposeAsync()
    {
        lock (_disposeLock)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
        }

        await Input.CompleteAsync();
        await Output.CompleteAsync();
    }

    protected override void Dispose(bool disposing)
    {
        throw new NotSupportedException();
    }
}