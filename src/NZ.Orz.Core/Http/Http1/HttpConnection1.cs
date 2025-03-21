using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Http.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZ.Orz.Http;

public class HttpConnection1 : HttpConnectionContext, IRequestProcessor
{
    public HttpConnection1(BaseConnectionContext connectionContext) : base(GatewayProtocols.HTTP1, connectionContext)
    {
    }

    public Task ProcessRequestsAsync(HttpConnectionDelegate application)
    {
        throw new NotImplementedException();
    }
}