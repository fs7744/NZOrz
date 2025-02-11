namespace NZ.Orz.Connections.Exceptions;

public class ConnectionAbortedException : OperationCanceledException
{
    public ConnectionAbortedException() :
    this("The connection was aborted")
    {
    }

    public ConnectionAbortedException(string message) : base(message)
    {
    }

    public ConnectionAbortedException(string message, Exception inner) : base(message, inner)
    {
    }
}