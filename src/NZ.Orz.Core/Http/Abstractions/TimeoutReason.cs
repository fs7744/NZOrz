namespace NZ.Orz.Http;

public enum TimeoutReason
{
    None,
    KeepAlive,
    RequestHeaders,
    ReadDataRate,
    WriteDataRate,
    RequestBodyDrain,
    TimeoutFeature,
}