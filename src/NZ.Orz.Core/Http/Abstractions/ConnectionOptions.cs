namespace NZ.Orz.Http;

[Flags]
public enum ConnectionOptions
{
    None = 0,
    Close = 1,
    KeepAlive = 2,
    Upgrade = 4
}