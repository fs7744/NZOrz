using NZ.Orz.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZ.Orz.Http.Http2;

internal sealed class Http2ConnectionErrorException : Exception
{
    public Http2ConnectionErrorException(string message, Http2ErrorCode errorCode, ConnectionEndReason reason)
        : base($"HTTP/2 connection error ({errorCode}): {message}")
    {
        ErrorCode = errorCode;
        Reason = reason;
    }

    public Http2ErrorCode ErrorCode { get; }
    public ConnectionEndReason Reason { get; }
}