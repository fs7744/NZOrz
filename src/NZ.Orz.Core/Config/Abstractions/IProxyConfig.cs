﻿namespace NZ.Orz.Config;

public interface IProxyConfig
{
    //private static readonly ConditionalWeakTable<IProxyConfig, string> _revisionIdsTable = new();

    //string RevisionId => _revisionIdsTable.GetValue(this, static _ => Guid.NewGuid().ToString());

    IReadOnlyList<RouteConfig> Routes { get; }

    IReadOnlyDictionary<string, ClusterConfig> Clusters { get; }

    IReadOnlyDictionary<string, ListenConfig> Listen { get; }
}