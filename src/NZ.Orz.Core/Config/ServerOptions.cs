namespace NZ.Orz.Config;

public class ServerOptions
{
    public ServerLimits Limits { get; } = new ServerLimits();

    public TimeSpan DefaultProxyTimeout { get; init; } = TimeSpan.FromSeconds(60);
}