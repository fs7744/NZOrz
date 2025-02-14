using NZ.Orz.Connections;
using System.Net;

namespace NZ.Orz.Config;

public class ListenOptions
{
    public string Key { get; set; }

    public IReadOnlyCollection<EndPoint> EndPoints { get; set; }

    public ConnectionDelegate ConnectionDelegate { get; set; }
}