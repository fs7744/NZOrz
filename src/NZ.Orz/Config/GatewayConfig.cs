namespace NZ.Orz.Config;

public class GatewayConfig
{
    public Dictionary<string, GatewayListenersConfig> Listeners { get; set; }
}

public class GatewayListenersConfig
{
    public string Address { get; set; }
    public GatewayProtocols Protocol { get; set; }

    public int Port { get; set; }

    public Dictionary<string, GatewayRouteRule> Rules { get; set; }
}

public enum GatewayProtocols
{
    TCP
}

public class GatewayRouteRule
{
    public List<GatewayUpstream> Backends { get; set; }
}

public class GatewayUpstream
{
    public string Address { get; set; }

    public int Port { get; set; }

    public decimal? Weight { get; set; }
}