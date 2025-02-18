using Microsoft.Extensions.Logging;
using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Sockets.Internal;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;

namespace NZ.Orz.Sockets;

public sealed class SocketConnectionContextFactory
{
    private readonly SocketTransportOptions options;
    private readonly ILogger _logger;
    private readonly int _settingsCount;
    private readonly QueueSettings[] _settings;
    private long _settingsIndex;

    public SocketConnectionContextFactory(IRouteContractor contractor, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(contractor);
        ArgumentNullException.ThrowIfNull(logger);

        options = contractor.GetSocketTransportOptions();
        ArgumentNullException.ThrowIfNull(options);
        _logger = logger;
        _settingsCount = options.IOQueueCount;

        var maxReadBufferSize = options.MaxReadBufferSize ?? 0;
        var maxWriteBufferSize = options.MaxWriteBufferSize ?? 0;
        var applicationScheduler = options.UnsafePreferInlineScheduling ? PipeScheduler.Inline : PipeScheduler.ThreadPool;

        if (_settingsCount > 0)
        {
            _settings = new QueueSettings[_settingsCount];

            for (var i = 0; i < _settingsCount; i++)
            {
                var memoryPool = options.MemoryPoolFactory();
                var transportScheduler = options.UnsafePreferInlineScheduling ? PipeScheduler.Inline : new IOQueue();

                _settings[i] = new QueueSettings()
                {
                    Scheduler = transportScheduler,
                    InputOptions = new PipeOptions(memoryPool, applicationScheduler, transportScheduler, maxReadBufferSize, maxReadBufferSize / 2, useSynchronizationContext: false),
                    OutputOptions = new PipeOptions(memoryPool, transportScheduler, applicationScheduler, maxWriteBufferSize, maxWriteBufferSize / 2, useSynchronizationContext: false),
                    SocketSenderPool = new SocketSenderPool(PipeScheduler.Inline),
                    MemoryPool = memoryPool,
                };
            }
        }
        else
        {
            var memoryPool = options.MemoryPoolFactory();
            var transportScheduler = options.UnsafePreferInlineScheduling ? PipeScheduler.Inline : PipeScheduler.ThreadPool;

            _settings = new QueueSettings[]
            {
                new QueueSettings()
                {
                    Scheduler = transportScheduler,
                    InputOptions = new PipeOptions(memoryPool, applicationScheduler, transportScheduler, maxReadBufferSize, maxReadBufferSize / 2, useSynchronizationContext: false),
                    OutputOptions = new PipeOptions(memoryPool, transportScheduler, applicationScheduler, maxWriteBufferSize, maxWriteBufferSize / 2, useSynchronizationContext: false),
                    SocketSenderPool = new SocketSenderPool(PipeScheduler.Inline),
                    MemoryPool = memoryPool,
                }
            };
            _settingsCount = 1;
        }
    }

    public ConnectionContext Create(Socket socket)
    {
        var setting = _settings[Interlocked.Increment(ref _settingsIndex) % _settingsCount];

        var connection = new SocketConnection(socket,
            setting.MemoryPool,
            setting.SocketSenderPool.Scheduler,
            _logger,
            setting.SocketSenderPool,
            setting.InputOptions,
            setting.OutputOptions,
            waitForData: options.WaitForDataBeforeAllocatingBuffer,
            finOnError: options.FinOnError);

        connection.Start();
        return connection;
    }

    public void Dispose()
    {
        foreach (var setting in _settings)
        {
            setting.SocketSenderPool.Dispose();
            setting.MemoryPool.Dispose();
        }
    }

    private sealed class QueueSettings
    {
        public PipeScheduler Scheduler { get; init; } = default!;
        public PipeOptions InputOptions { get; init; } = default!;
        public PipeOptions OutputOptions { get; init; } = default!;
        public SocketSenderPool SocketSenderPool { get; init; } = default!;
        public MemoryPool<byte> MemoryPool { get; init; } = default!;
    }
}