using Microsoft.Extensions.Primitives;
using System.Runtime.CompilerServices;

namespace NZ.Orz.Config;

public interface IProxyConfig
{
    private static readonly ConditionalWeakTable<IProxyConfig, string> _revisionIdsTable = new();

    string RevisionId => _revisionIdsTable.GetValue(this, static _ => Guid.NewGuid().ToString());

    IReadOnlyList<RouteConfig> Routes { get; }

    IReadOnlyList<ClusterConfig> Clusters { get; }

    IChangeToken ChangeToken { get; }
}