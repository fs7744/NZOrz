using Microsoft.Extensions.Hosting;
using NZ.Orz;
using NZ.Orz.Config;

var app = NZApp.CreateBuilder(args)
    .ConfigureMemoryRouteConfig(new RouteConfig()
    {
        { "Test", new GatewayConfig ()
            {
                 Listeners = new Dictionary<string, GatewayListenersConfig>()
                 {
                     { "Listener1", new GatewayListenersConfig()
                        {
                            Address = "127.0.0.1",
                            Protocol = GatewayProtocols.TCP,
                            Port = 5000,
                            Rules = new Dictionary<string, GatewayRouteRule>()
                            {
                                {"Rule1", new GatewayRouteRule()
                                    {
                                        Backends = new List<GatewayUpstream>() { new GatewayUpstream() { Address = "14.215.177.38", Port = 80,  } }
                                    }
                                }
                            }
                         }
                     }
                 }
            }
        }
    }).Build();

await app.RunAsync();