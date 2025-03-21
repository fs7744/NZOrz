using NZ.Orz.Config;
using NZ.Orz.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZ.Orz.ReverseProxy.Http;

public class HttpRouter : IHttpRouter
{
    public Task ReBulidAsync(IProxyConfig proxyConfig, ServerOptions options)
    {
        // todo
        return Task.CompletedTask;
    }
}