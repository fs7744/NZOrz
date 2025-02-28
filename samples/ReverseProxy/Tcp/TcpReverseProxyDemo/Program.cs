using Microsoft.Extensions.Hosting;
using NZ.Orz;
using NZ.Orz.Config;
using NZ.Orz.Config.Memory;

var app = NZApp.CreateBuilder(args)
    .UseMemoryConfig(b =>
    {
        b.Routes.Add(new MemoryRouteConfig()
        {
            Protocols = GatewayProtocols.TCP,
            Match = new MemoryRouteMatch()
            {
                Hosts = new List<string>() { "localhost:5000" }
            },
            ClusterId = "apidemo"
        });

        b.Clusters.Add(new MemoryClusterConfig()
        {
            ClusterId = "apidemo",
            Destinations = new List<DestinationConfig> { new DestinationConfig() { Address = "localhost:5144" } }
        });
    })
    .Build();

await app.RunAsync();