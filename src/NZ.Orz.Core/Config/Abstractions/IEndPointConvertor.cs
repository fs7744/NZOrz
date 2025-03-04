using System.Net;

namespace NZ.Orz.Config;

public interface IEndPointConvertor
{
    int Order { get; }

    public bool TryConvert(string address, out IEnumerable<EndPoint> endPoint);
}