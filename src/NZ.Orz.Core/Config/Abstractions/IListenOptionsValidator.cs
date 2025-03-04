namespace NZ.Orz.Config;

public interface IListenOptionsValidator
{
    int Order { get; }

    public ValueTask ValidateAsync(ListenOptions options, IList<Exception> errors, CancellationToken cancellationToken);
}