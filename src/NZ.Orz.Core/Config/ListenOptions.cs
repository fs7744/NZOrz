using NZ.Orz.Connections;
using System.Net;

namespace NZ.Orz.Config;

public class ListenOptions
{
    public string Key { get; set; }

    public GatewayProtocols Protocols { get; set; }

    public EndPoint EndPoint { get; set; }

    public ConnectionDelegate ConnectionDelegate { get; set; }
    public MultiplexedConnectionDelegate MultiplexedConnectionDelegate { get; set; }

    public bool Equals(ListenOptions? obj)
    {
        if (obj is null) return false;
        return Key.Equals(obj.Key, StringComparison.OrdinalIgnoreCase)
            && Protocols == obj.Protocols
            && EndPoint.GetHashCode() == EndPoint.GetHashCode();
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Key?.GetHashCode(StringComparison.OrdinalIgnoreCase), Protocols, EndPoint.GetHashCode());
    }

    public override string ToString()
    {
        return $"[Protocols: {Protocols},Route: {Key},EndPoint: {EndPoint}]";
    }
}