﻿using Microsoft.Extensions.Primitives;
using NZ.Orz.Sockets;

namespace NZ.Orz.Config.Customize;

internal class MemoryRouteConfigContractor : IRouteContractor
{
    private readonly ServerOptions serverOptions;
    private ListenOptions[] listenOptions;
    private Func<IServiceProvider, ListenOptions[]> listenOptionsFactory;
    private readonly SocketTransportOptions socketTransportOptions;

    public IServiceProvider ServiceProvider { get; internal set; }

    public MemoryRouteConfigContractor(ServerOptions serverOptions, Func<IServiceProvider, ListenOptions[]> listenOptions, SocketTransportOptions socketTransportOptions)
    {
        this.serverOptions = serverOptions;
        this.listenOptionsFactory = listenOptions;
        this.socketTransportOptions = socketTransportOptions;
    }

    public IEnumerable<ListenOptions> GetListenOptions()
    {
        return listenOptions;
    }

    public IChangeToken? GetReloadToken()
    {
        return null;
    }

    public ServerOptions GetServerOptions()
    {
        return serverOptions;
    }

    public SocketTransportOptions? GetSocketTransportOptions()
    {
        return socketTransportOptions;
    }

    public Task LoadAsync(CancellationToken cancellationToken)
    {
        this.listenOptions = listenOptionsFactory(ServiceProvider);
        listenOptionsFactory = null;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}