using NZ.Orz.Buffers;
using NZ.Orz.Connections;
using NZ.Orz.Connections.Exceptions;
using NZ.Orz.Connections.Features;
using NZ.Orz.Infrastructure;
using NZ.Orz.Metrics;
using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net.Sockets;

namespace NZ.Orz.Sockets.Internal;

internal sealed partial class SocketConnection : TransportConnection, IConnectionSocketFeature
{
    private static readonly int MinAllocBufferSize = PinnedBlockMemoryPool.BlockSize / 2;

    private readonly Socket _socket;
    private readonly OrzLogger _logger;
    private readonly SocketReceiver _receiver;
    private SocketSender? _sender;
    private readonly SocketSenderPool _socketSenderPool;
    private readonly IDuplexPipe _originalTransport;
    private readonly CancellationTokenSource _connectionClosedTokenSource = new CancellationTokenSource();
#if NET9_0_OR_GREATER
    private readonly Lock _shutdownLock = new();
#else
    private readonly object _shutdownLock = new();
#endif

    private volatile Exception? _shutdownReason;
    private Task? _sendingTask;
    private Task? _receivingTask;
    private readonly TaskCompletionSource _waitForConnectionClosedTcs = new TaskCompletionSource();
    private bool _connectionClosed;
    private readonly bool _waitForData;
    private readonly bool _finOnError;

    internal SocketConnection(Socket socket,
                              MemoryPool<byte> memoryPool,
                              PipeScheduler socketScheduler,
                              OrzLogger logger,
                              SocketSenderPool socketSenderPool,
                              PipeOptions inputOptions,
                              PipeOptions outputOptions,
                              bool waitForData = true,
                              bool finOnError = false)
    {
        Debug.Assert(socket != null);
        Debug.Assert(memoryPool != null);
        Debug.Assert(logger != null);

        _socket = socket;
        MemoryPool = memoryPool;
        _logger = logger;
        _waitForData = waitForData;
        _socketSenderPool = socketSenderPool;
        _finOnError = finOnError;

        LocalEndPoint = _socket.LocalEndPoint;
        RemoteEndPoint = _socket.RemoteEndPoint;

        ConnectionClosed = _connectionClosedTokenSource.Token;

        _receiver = new SocketReceiver(socketScheduler);

        var pair = DuplexPipe.CreateConnectionPair(inputOptions, outputOptions);

        // Set the transport and connection id
        Transport = _originalTransport = pair.Transport;
        Application = pair.Application;
    }

    internal override void InitializeFeatures()
    {
        Parameters.SetFeature<IConnectionSocketFeature>(this);
    }

    public PipeWriter Input => Application.Output;

    public PipeReader Output => Application.Input;

    public override MemoryPool<byte> MemoryPool { get; }

    public Socket Socket => _socket;

    public override string TransportType => "tcp";

    public void Start()
    {
        try
        {
            // Spawn send and receive logic
            _receivingTask = DoReceive();
            _sendingTask = DoSend();
        }
        catch (Exception ex)
        {
            _logger.UnexpectedException($"in {nameof(SocketConnection)}.{nameof(Start)}.", ex);
        }
    }

    public override void Abort(ConnectionAbortedException abortReason)
    {
        // Try to gracefully close the socket to match libuv behavior.
        Shutdown(abortReason);

        // Cancel ProcessSends loop after calling shutdown to ensure the correct _shutdownReason gets set.
        Output.CancelPendingRead();
    }

    // Only called after connection middleware is complete which means the ConnectionClosed token has fired.
    public override async ValueTask DisposeAsync()
    {
        _originalTransport.Input.Complete();
        _originalTransport.Output.Complete();

        try
        {
            // Now wait for both to complete
            if (_receivingTask != null)
            {
                await _receivingTask;
            }

            if (_sendingTask != null)
            {
                await _sendingTask;
            }
        }
        catch (Exception ex)
        {
            _logger.UnexpectedException($"in {nameof(SocketConnection)}.{nameof(Start)}.", ex);
        }
        finally
        {
            _receiver.Dispose();
            _sender?.Dispose();
        }

        _connectionClosedTokenSource.Dispose();
    }

    private async Task DoReceive()
    {
        Exception? error = null;

        try
        {
            while (_shutdownReason is null)
            {
                if (_waitForData)
                {
                    // Wait for data before allocating a buffer.
                    var waitForDataResult = await _receiver.WaitForDataAsync(_socket);

                    if (!IsNormalCompletion(waitForDataResult))
                    {
                        break;
                    }
                }

                // Ensure we have some reasonable amount of buffer space
                var buffer = Input.GetMemory(MinAllocBufferSize);

                var receiveResult = await _receiver.ReceiveAsync(_socket, buffer);

                if (!IsNormalCompletion(receiveResult))
                {
                    break;
                }

                var bytesReceived = receiveResult.BytesTransferred;

                if (bytesReceived == 0)
                {
                    // FIN
                    _logger.ConnectionReadFin(this.ConnectionId);
                    break;
                }

                Input.Advance(bytesReceived);

                var flushTask = Input.FlushAsync();

                var paused = !flushTask.IsCompleted;

                if (paused)
                {
                    _logger.ConnectionPause(ConnectionId);
                }

                var result = await flushTask;

                if (paused)
                {
                    _logger.ConnectionResume(ConnectionId);
                }

                if (result.IsCompleted || result.IsCanceled)
                {
                    // Pipe consumer is shut down, do we stop writing
                    break;
                }

                bool IsNormalCompletion(SocketOperationResult result)
                {
                    // There's still a small chance that both DoReceive() and DoSend() can log the same connection reset.
                    // Both logs will have the same ConnectionId. I don't think it's worthwhile to lock just to avoid this.
                    // When _shutdownReason is set, error is ignored, so it does not need to be initialized.
                    if (_shutdownReason is not null)
                    {
                        return false;
                    }

                    if (!result.HasError)
                    {
                        return true;
                    }

                    if (IsConnectionResetError(result.SocketError.SocketErrorCode))
                    {
                        var ex = result.SocketError;
                        error = new ConnectionResetException(ex.Message, ex);

                        _logger.ConnectionReset(ConnectionId);

                        return false;
                    }

                    if (IsConnectionAbortError(result.SocketError.SocketErrorCode))
                    {
                        error = result.SocketError;

                        // This is unexpected if the socket hasn't been disposed yet.
                        _logger.ConnectionError(ConnectionId, error);

                        return false;
                    }

                    // This is unexpected.
                    error = result.SocketError;
                    _logger.ConnectionError(ConnectionId, error);

                    return false;
                }
            }
        }
        catch (ObjectDisposedException ex)
        {
            // This exception should always be ignored because _shutdownReason should be set.
            error = ex;

            if (_shutdownReason is not null)
            {
                // This is unexpected if the socket hasn't been disposed yet.
                _logger.ConnectionError(ConnectionId, error);
            }
        }
        catch (Exception ex)
        {
            // This is unexpected.
            error = ex;
            _logger.ConnectionError(ConnectionId, error);
        }
        finally
        {
            // If Shutdown() has already been called, assume that was the reason ProcessReceives() exited.
            Input.Complete(_shutdownReason ?? error);

            FireConnectionClosed();

            await _waitForConnectionClosedTcs.Task;
        }
    }

    private async Task DoSend()
    {
        Exception? shutdownReason = null;
        Exception? unexpectedError = null;

        try
        {
            while (true)
            {
                var result = await Output.ReadAsync();

                if (result.IsCanceled)
                {
                    break;
                }
                var buffer = result.Buffer;

                if (!buffer.IsEmpty)
                {
                    _sender = _socketSenderPool.Rent();
                    var transferResult = await _sender.SendAsync(_socket, buffer);

                    if (transferResult.HasError)
                    {
                        if (IsConnectionResetError(transferResult.SocketError.SocketErrorCode))
                        {
                            var ex = transferResult.SocketError;
                            shutdownReason = new ConnectionResetException(ex.Message, ex);
                            _logger.ConnectionReset(this.ConnectionId);

                            break;
                        }

                        if (IsConnectionAbortError(transferResult.SocketError.SocketErrorCode))
                        {
                            shutdownReason = transferResult.SocketError;

                            break;
                        }

                        unexpectedError = shutdownReason = transferResult.SocketError;
                    }

                    // We don't return to the pool if there was an exception, and
                    // we keep the _sender assigned so that we can dispose it in StartAsync.
                    _socketSenderPool.Return(_sender);
                    _sender = null;
                }

                Output.AdvanceTo(buffer.End);

                if (result.IsCompleted)
                {
                    break;
                }
            }
        }
        catch (ObjectDisposedException ex)
        {
            // This should always be ignored since Shutdown() must have already been called by Abort().
            shutdownReason = ex;
        }
        catch (Exception ex)
        {
            shutdownReason = ex;
            unexpectedError = ex;
            _logger.ConnectionError(ConnectionId, unexpectedError);
        }
        finally
        {
            Shutdown(shutdownReason);

            // Complete the output after disposing the socket
            Output.Complete(unexpectedError);

            // Cancel any pending flushes so that the input loop is un-paused
            Input.CancelPendingFlush();
        }
    }

    private void FireConnectionClosed()
    {
        // Guard against scheduling this multiple times
        if (_connectionClosed)
        {
            return;
        }

        _connectionClosed = true;

        ThreadPool.UnsafeQueueUserWorkItem(state =>
        {
            state.CancelConnectionClosedToken();

            state._waitForConnectionClosedTcs.TrySetResult();
        },
        this,
        preferLocal: false);
    }

    private void Shutdown(Exception? shutdownReason)
    {
        lock (_shutdownLock)
        {
            if (_shutdownReason is not null)
            {
                return;
            }

            // Make sure to dispose the socket after the volatile _shutdownReason is set.
            // Without this, the RequestsCanBeAbortedMidRead test will sometimes fail when
            // a BadHttpRequestException is thrown instead of a TaskCanceledException.
            //
            // The shutdownReason argument should only be null if the output was completed gracefully, so no one should ever
            // ever observe this ConnectionAbortedException except for connection middleware attempting
            // to half close the connection which is currently unsupported. The message is always logged though.
            _shutdownReason = shutdownReason ?? new ConnectionAbortedException("The Socket transport's send loop completed gracefully.");

            // NB: not _shutdownReason since we don't want to do this on graceful completion
            if (!_finOnError && shutdownReason is not null)
            {
                _logger.ConnectionWriteRst(this.ConnectionId, shutdownReason.Message);

                // This forces an abortive close with linger time 0 (and implies Dispose)
                _socket.Close(timeout: 0);
                return;
            }

            _logger.ConnectionWriteFin(this.ConnectionId, _shutdownReason.Message);

            try
            {
                _socket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
                // Ignore any errors from Socket.Shutdown() since we're tearing down the connection anyway.
            }

            _socket.Dispose();
        }
    }

    private void CancelConnectionClosedToken()
    {
        try
        {
            _connectionClosedTokenSource.Cancel();
        }
        catch (Exception ex)
        {
            _logger.UnexpectedException($"in {nameof(SocketConnection)}.{nameof(CancelConnectionClosedToken)}.", ex);
        }
    }

    private static bool IsConnectionResetError(SocketError errorCode)
    {
        return errorCode == SocketError.ConnectionReset ||
               errorCode == SocketError.Shutdown ||
               (errorCode == SocketError.ConnectionAborted && OperatingSystem.IsWindows());
    }

    private static bool IsConnectionAbortError(SocketError errorCode)
    {
        // Calling Dispose after ReceiveAsync can cause an "InvalidArgument" error on *nix.
        return errorCode == SocketError.OperationAborted ||
               errorCode == SocketError.Interrupted ||
               (errorCode == SocketError.InvalidArgument && !OperatingSystem.IsWindows());
    }
}