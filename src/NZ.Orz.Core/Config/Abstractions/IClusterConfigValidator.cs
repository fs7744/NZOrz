namespace NZ.Orz.Config;

public interface IClusterConfigValidator
{
    public ValueTask ValidateAsync(ClusterConfig cluster, IList<Exception> errors);
}
