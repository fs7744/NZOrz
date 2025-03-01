namespace NZ.Orz.Config;

public interface IListenOptionsValidator
{
    public ValueTask ValidateAsync(ListenOptions options, IList<Exception> errors, CancellationToken cancellationToken);
}