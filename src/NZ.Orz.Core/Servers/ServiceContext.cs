﻿using NZ.Orz.Config;
using NZ.Orz.Connections;
using NZ.Orz.Infrastructure;
using NZ.Orz.Metrics;
using System.IO.Pipelines;

namespace NZ.Orz.Servers;

public class ServiceContext
{
    public OrzLogger Log { get; set; } = default!;

    public PipeScheduler Scheduler { get; set; } = default!;

    public TimeProvider TimeProvider { get; set; } = default!;

    public Heartbeat Heartbeat { get; set; } = default!;

    public ServerOptions ServerOptions { get; set; } = default!;

    public OrzMetrics Metrics { get; set; } = default!;
    public ConnectionManager ConnectionManager { get; set; } = default!;
}