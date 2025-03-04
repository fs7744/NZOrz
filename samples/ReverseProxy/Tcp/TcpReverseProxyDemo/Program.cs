﻿using Microsoft.Extensions.Hosting;
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
                Hosts = new List<string>() { "*:5000" }
            },
            ClusterId = "apidemo"
        });

        b.Clusters.Add(new MemoryClusterConfig()
        {
            LoadBalancingPolicy = "Random",
            ClusterId = "apidemo",
            Destinations = new List<DestinationConfig>
            {
                new DestinationConfig() { Address = "[::1]:5144" },
                new DestinationConfig() { Address = "google.com:998" }, new DestinationConfig() { Address = "google.com" } , new DestinationConfig() { Address = "http://google.com" }, new DestinationConfig() { Address = "https://google.com" }
            }
        });
    })
    .Build();

await app.RunAsync();