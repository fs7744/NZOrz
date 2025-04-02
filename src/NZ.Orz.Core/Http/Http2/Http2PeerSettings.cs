namespace NZ.Orz.Http.Http2;

/// <summary>
/// Tracks HTTP/2 settings for a peer (client or server).
/// </summary>
/// <remarks>
/// There are expected to be exactly two instances, one for the client and one for the server.
/// Both are owned by the <see cref="Http2Connection"/>.
/// </remarks>
/// <seealso href="https://datatracker.ietf.org/doc/html/rfc9113#name-defined-settings"/>
internal sealed class Http2PeerSettings
{
    // Note these are protocol defaults
    public const uint DefaultHeaderTableSize = 4096;

    public const bool DefaultEnablePush = true;
    public const uint DefaultMaxConcurrentStreams = uint.MaxValue;
    public const uint DefaultInitialWindowSize = 65535;
    public const uint DefaultMaxFrameSize = MinAllowedMaxFrameSize;
    public const uint DefaultMaxHeaderListSize = uint.MaxValue;
    public const uint MaxWindowSize = int.MaxValue;
    internal const int MinAllowedMaxFrameSize = 16 * 1024;
    internal const int MaxAllowedMaxFrameSize = 16 * 1024 * 1024 - 1;

    //public uint HeaderTableSize { get; set; } = DefaultHeaderTableSize;

    //public bool EnablePush { get; set; } = DefaultEnablePush;

    //public uint MaxConcurrentStreams { get; set; } = DefaultMaxConcurrentStreams;

    //public uint InitialWindowSize { get; set; } = DefaultInitialWindowSize;

    //public uint MaxFrameSize { get; set; } = DefaultMaxFrameSize;

    //public uint MaxHeaderListSize { get; set; } = DefaultMaxHeaderListSize;

    // TODO: Return the diff so we can react
}