using System.Collections;

namespace NZOrz.Features;

public class FeatureCollection : IFeatureCollection
{
    private readonly int _initialCapacity;
    private readonly IFeatureCollection? _defaults;
    private IDictionary<object, object>? _features;

    public FeatureCollection(IFeatureCollection? defaults = null, int initialCapacity = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialCapacity, nameof(initialCapacity));
        _initialCapacity = initialCapacity;
        _defaults = defaults;
    }

    public object? this[object key]
    {
        get
        {
            return _features != null && _features.TryGetValue(key, out var result) ? result : _defaults?[key];
        }
        set
        {
            if (value == null)
            {
                _features?.Remove(key);
                return;
            }

            _features ??= new Dictionary<object, object>(_initialCapacity);
            _features[key] = value;
        }
    }

    public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
    {
        if (_features != null)
        {
            foreach (var pair in _features)
            {
                yield return pair;
            }
        }

        if (_defaults != null)
        {
            foreach (var pair in _features == null ? _defaults : _defaults.Except(_features))
            {
                yield return pair;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public TFeature? GetFeature<TFeature>()
    {
        return (TFeature?)this[typeof(TFeature)];
    }

    public void SetFeature<TFeature>(TFeature? instance)
    {
        this[typeof(TFeature)] = instance;
    }
}