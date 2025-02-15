using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace NZ.Orz.Connections;

public sealed class NamedPipeEndPoint : EndPoint
{
    internal const string LocalComputerServerName = ".";

    public NamedPipeEndPoint(string pipeName) : this(pipeName, LocalComputerServerName)
    {
    }

    public NamedPipeEndPoint(string pipeName, string serverName)
    {
        ServerName = serverName;
        PipeName = pipeName;
    }

    public string ServerName { get; }

    public string PipeName { get; }

    public override string ToString()
    {
        // Based on format at https://learn.microsoft.com/windows/win32/ipc/pipe-names
        return $@"\\{ServerName}\pipe\{PipeName}";
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is NamedPipeEndPoint other && other.ServerName == ServerName && other.PipeName == PipeName;
    }

    public override int GetHashCode()
    {
        return ServerName.GetHashCode() ^ PipeName.GetHashCode();
    }
}