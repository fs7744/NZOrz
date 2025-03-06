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
}