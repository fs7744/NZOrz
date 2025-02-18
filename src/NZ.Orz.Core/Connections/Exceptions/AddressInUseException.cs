namespace NZ.Orz.Connections.Exceptions;

public class AddressInUseException : InvalidOperationException
{
    public AddressInUseException(string message) : base(message)
    {
    }

    public AddressInUseException(string message, Exception inner) : base(message, inner)
    {
    }
}