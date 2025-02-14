using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NZ.Orz;
using System.Net;
using TcpDemo;

var app = NZApp.CreateBuilder(args)
    .ConfigureRoute(b =>
    {
        b.AddEndPoint("test", i =>
        {
            i.Services.AddSingleton<TestProxyHandler>();
            i.Listen(IPEndPoint.Parse("127.0.0.1:5000")).UseMiddleware<TestProxyHandler>();
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