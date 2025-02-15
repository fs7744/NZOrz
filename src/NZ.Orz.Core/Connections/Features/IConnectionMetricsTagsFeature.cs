namespace NZ.Orz.Connections.Features;

public interface IConnectionMetricsTagsFeature
{
    ICollection<KeyValuePair<string, object?>> Tags { get; }
}