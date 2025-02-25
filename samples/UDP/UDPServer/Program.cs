using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NZ.Orz;
using NZ.Orz.Config;
using NZ.Orz.Config.Customize;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UDPServer;

static void StartListener()
{
    UdpClient listener = new UdpClient(11000);
    IPEndPoint groupEP = null;

    try
    {
        while (true)
        {
            Console.WriteLine("Waiting for broadcast");
            byte[] bytes = listener.Receive(ref groupEP);

            var data = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
            Console.WriteLine($"Received broadcast from {groupEP} :");
            Console.WriteLine($" {data}");
            listener.Send(Encoding.ASCII.GetBytes(data.Reverse().ToArray()), groupEP);
        }
    }
    catch (SocketException e)
    {
        Console.WriteLine(e);
    }
    finally
    {
        listener.Close();
    }
}

static void Client(string[] args)
{
    var client = new UdpClient();

    IPAddress broadcast = IPAddress.Parse("127.0.0.1");
    byte[] sendbuf = Encoding.ASCII.GetBytes(args[1]);
    IPEndPoint ep = new IPEndPoint(broadcast, 5000);

    client.Send(sendbuf, ep);
    Console.WriteLine("Message sent to the broadcast address");

    sendbuf = client.Receive(ref ep);

    Console.WriteLine($"Receive Message : {Encoding.ASCII.GetString(sendbuf)}");
}

static void Proxy(string[] args)
{
    var builder = NZApp.CreateBuilder(args);
    builder.ApplicationBuilder
    .ConfigureRoute(b =>
    {
        b.AddEndPoint("test", i =>
        {
            i.Protocols = GatewayProtocols.UDP;
            i.Services.AddSingleton<TestProxyHandler>();
            i.Listen(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000)).UseMiddleware<TestProxyHandler>();
        });
    });
    var app = builder.Build();

    app.RunAsync().GetAwaiter().GetResult();
}

if (args.Length == 0)
{
    args = new string[] { "proxy", "test" };
}
if (args[0].Equals("server"))
{
    StartListener();
}
else if (args[0].Equals("client"))
{
    Client(args);
}
else
{
    Proxy(args);
}