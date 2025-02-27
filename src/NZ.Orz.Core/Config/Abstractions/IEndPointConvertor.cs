using System.Net;

namespace NZ.Orz.Config.Abstractions;

public interface IEndPointConvertor
{
    public bool TryConvert(string address, out EndPoint endPoint);
}