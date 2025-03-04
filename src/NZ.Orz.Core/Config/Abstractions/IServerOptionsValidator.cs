namespace NZ.Orz.Config;

public interface IServerOptionsValidator
{
    int Order { get; }

    public ValueTask ValidateAsync(ServerOptions options, IList<Exception> errors, CancellationToken cancellationToken);
}