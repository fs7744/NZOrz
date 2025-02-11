namespace NZ.Orz.Features;

public interface IFeatureCollection : IEnumerable<KeyValuePair<object, object>>
{
    object? this[object key] { get; set; }

    TFeature? GetFeature<TFeature>();

    void SetFeature<TFeature>(TFeature? instance);
}