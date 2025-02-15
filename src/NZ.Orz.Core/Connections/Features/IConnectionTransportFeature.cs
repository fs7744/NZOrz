using System.IO.Pipelines;

namespace NZ.Orz.Connections.Features;

public interface IConnectionTransportFeature
{
    IDuplexPipe Transport { get; set; }
}