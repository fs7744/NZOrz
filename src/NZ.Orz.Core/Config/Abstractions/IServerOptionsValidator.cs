namespace NZ.Orz.Config;

public interface IServerOptionsValidator
{
    public ValueTask ValidateAsync(ServerOptions options, IList<Exception> errors, CancellationToken cancellationToken);
}