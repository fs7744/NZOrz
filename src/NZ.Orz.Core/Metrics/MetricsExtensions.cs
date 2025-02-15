using NZ.Orz.Connections.Features;
using System.Diagnostics;

namespace NZ.Orz.Metrics;

public static class MetricsExtensions
{
    public static bool TryAddTag(this IConnectionMetricsTagsFeature feature, string name, object? value)
    {
        var tags = feature.Tags;

        return TryAddTagCore(name, value, tags);
    }

    private static bool TryAddTagCore(string name, object? value, ICollection<KeyValuePair<string, object?>> tags)
    {
        // Tags is internally represented as a List<T>.
        // Prefer looping through the list to avoid allocating an enumerator.
        if (tags is List<KeyValuePair<string, object?>> list)
        {
            foreach (var tag in list)
            {
                if (tag.Key == name)
                {
                    return false;
                }
            }
        }
        else
        {
            foreach (var tag in tags)
            {
                if (tag.Key == name)
                {
                    return false;
                }
            }
        }

        tags.Add(new KeyValuePair<string, object?>(name, value));
        return true;
    }

    public static bool TryAddTag(this ref TagList tags, string name, object? value)
    {
        for (var i = 0; i < tags.Count; i++)
        {
            if (tags[i].Key == name)
            {
                return false;
            }
        }

        tags.Add(new KeyValuePair<string, object?>(name, value));
        return true;
    }
}