using NZ.Orz.Connections.Exceptions;
using NZ.Orz.Connections.Features;
using System.IO.Pipelines;

namespace NZ.Orz.Connections;

public abstract class ConnectionContext : BaseConnectionContext, IAsyncDisposable
{
    public abstract IDuplexPipe Transport { get; set; }

    public override void Abort(ConnectionAbortedException abortReason)
    {
        Parameters.GetFeature<IConnectionLifetimeFeature>()?.Abort();
    }

    public override void Abort() => Abort(new ConnectionAbortedException("The connection was aborted by the application via ConnectionContext.Abort()."));
}