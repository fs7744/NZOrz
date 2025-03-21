using Microsoft.Extensions.Hosting;
using NZ.Orz;

var app = NZApp.CreateBuilder(args)
    .UseJsonConfig()
    .Build();

await app.RunAsync();