using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NZ.Orz.Config.Customize;

public class RouteConfigBuilder
{
    public IServiceProvider ServiceProvider { get; internal set; }

    internal List<ListenOptionsBuilder> EndPoints { get; private set; } = new List<ListenOptionsBuilder>();

    public void AddEndPoint(Action<ListenOptionsBuilder> action)
    {
        var builder = new ListenOptionsBuilder();
        EndPoints.Add(builder);
        builder.ServiceProvider = ServiceProvider;
        action(builder);
    }
}