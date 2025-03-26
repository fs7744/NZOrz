namespace NZ.Orz.Http;

public enum HttpVersion : sbyte
{
    Unknown = -1,
    Http10 = 0,
    Http11 = 1,
    Http2 = 2,
    Http3 = 3
}