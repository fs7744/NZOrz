namespace NZ.Orz.Config;

public interface IProxyConfig
{
    //private static readonly ConditionalWeakTable<IProxyConfig, string> _revisionIdsTable = new();

    //string RevisionId => _revisionIdsTable.GetValue(this, static _ => Guid.NewGuid().ToString());

    IList<RouteConfig> Routes { get; }

    IList<ClusterConfig> Clusters { get; }
}