namespace NZ.Orz.Config;

public class ServerLimits
{
    public long? MaxConcurrentConnections { get; set; }

    public long? MaxConcurrentUpgradedConnections { get; set; }

    public TimeSpan KeepAliveTimeout { get; set; } = TimeSpan.FromSeconds(130);

    public long? MaxResponseBufferSize { get; set; } = 64 * 1024;

    public long? MaxRequestBufferSize { get; set; } = 1024 * 1024;

    public int MaxRequestLineSize { get; set; } = 8 * 1024;

    public int MaxRequestHeadersTotalSize { get; set; } = 32 * 1024;

    public int MaxRequestHeaderCount { get; set; } = 100;

    /// <summary>
    /// (~28.6 MB)
    /// </summary>
    public long? MaxRequestBodySize { get; set; } = 30000000;

    public TimeSpan RequestHeadersTimeout { get; set; } = TimeSpan.FromSeconds(30);

    public MinDataRate? MinRequestBodyDataRate { get; set; } =
        new MinDataRate(bytesPerSecond: 240, gracePeriod: TimeSpan.FromSeconds(5));

    public MinDataRate? MinResponseDataRate { get; set; } =
        new MinDataRate(bytesPerSecond: 240, gracePeriod: TimeSpan.FromSeconds(5));

    //todo http2 3 limit
}