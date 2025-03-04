namespace NZ.Orz.Config;

public interface IClusterConfigValidator
{
    int Order { get; }

    public ValueTask ValidateAsync(ClusterConfig cluster, IList<Exception> errors, CancellationToken cancellationToken);
}