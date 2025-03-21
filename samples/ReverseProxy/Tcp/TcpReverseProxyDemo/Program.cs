﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NZ.Orz;
using NZ.Orz.ReverseProxy.L4;

var app = NZApp.CreateBuilder(args)
    .ConfigServices(services =>
    {
        services.AddSingleton<ITcpMiddleware, EchoMiddleware>();
        services.AddSingleton<IUdpMiddleware, UdpEchoMiddleware>();
    })
    .UseJsonConfig()
    .Build();

await app.RunAsync();