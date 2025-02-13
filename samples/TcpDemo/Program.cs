using Microsoft.Extensions.Hosting;
using NZ.Orz;

var app = NZApp.CreateBuilder(args)
    .ConfigureRoute(b =>
    {
        b.AddEndPoint(i =>
        {
        });
    })
    //.ConfigureMemoryRouteConfig(new RouteConfig()
    //{
    //    { new GatewayConfig ()
    //        {
    //             Listeners = new List<GatewayListenersConfig>()
    //             {
    //                 new GatewayListenersConfig()
    //                {
    //                    Address = "127.0.0.1",
    //                    Protocol = GatewayProtocols.TCP,
    //                    Port = 5000,
    //                    Rules = new List<GatewayRouteRule>()
    //                    {
    //                        new GatewayRouteRule()
    //                        {
    //                            Backends = new List<GatewayUpstream>() { new GatewayUpstream() { Address = "14.215.177.38", Port = 80,  } }
    //                        }
    //                    }
    //                }
    //             }
    //        }
    //    }
    //})
    .Build();

await app.RunAsync();