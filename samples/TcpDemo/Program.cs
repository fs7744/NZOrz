using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NZ.Orz;
using NZ.Orz.Config;
using NZ.Orz.Config.Customize;
using System.Net;
using TcpDemo;

var builder = NZApp.CreateBuilder(args);
builder.ApplicationBuilder
.ConfigureRoute(b =>
{
    b.AddEndPoint("test", i =>
    {
        i.Protocols = GatewayProtocols.TCP;
        i.Services.AddSingleton<TestProxyHandler>();
        i.Listen(IPEndPoint.Parse("[0000:0000:0000:0000:0000:0000:0000:0001]:5000"), IPEndPoint.Parse("127.0.0.1:5001")).UseMiddleware<TestProxyHandler>();
    });
});
var app = builder.Build();

await app.RunAsync();