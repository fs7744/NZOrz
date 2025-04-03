using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZ.Orz.Http;

internal sealed class BodyControl
{
    private static readonly ThrowingWasUpgradedWriteOnlyStream _throwingResponseStream = new();
    private static readonly ThrowingPipeWriter _throwingUpgradedPipeWriter = new("Cannot write to response body after connection has been upgraded.");
    private readonly HttpResponseStream _response;
    private readonly HttpResponsePipeWriter _responseWriter;
    private readonly HttpRequestPipeReader _requestReader;
    private readonly HttpRequestStream _request;
    private readonly HttpRequestPipeReader _emptyRequestReader;
    private readonly WrappingStream _upgradeableResponse;
    private readonly WrappingPipeWriter _upgradeablePipeWriter;
    private readonly StatusCheckPipeWriter _connectPipeWriter;
    private readonly StatusCheckWriteStream _connectResponse;
    private readonly HttpRequestStream _emptyRequest;
    private readonly Stream _upgradeStream;

    public BodyControl(IHttpResponseControl responseControl)
    {
        _requestReader = new HttpRequestPipeReader();
        _request = new HttpRequestStream(_requestReader);
        _emptyRequestReader = new HttpRequestPipeReader();
        _emptyRequest = new HttpRequestStream(_emptyRequestReader);

        _responseWriter = new HttpResponsePipeWriter(responseControl);
        _response = new HttpResponseStream(_responseWriter);
        _upgradeableResponse = new WrappingStream(_response);
        _upgradeablePipeWriter = new WrappingPipeWriter(_responseWriter);
        _upgradeStream = new HttpUpgradeStream(_request, _response);
        _connectPipeWriter = new(_responseWriter);
        _connectResponse = new(_response);
    }

    public bool CanHaveBody { get; private set; }

    public Stream Upgrade()
    {
        // causes writes to context.Response.Body to throw
        _upgradeableResponse.SetInnerStream(_throwingResponseStream);
        _upgradeablePipeWriter.SetInnerPipe(_throwingUpgradedPipeWriter);
        // _upgradeStream always uses _response
        return _upgradeStream;
    }

    public Stream AcceptConnect()
    {
        return _upgradeStream;
    }

    public (Stream request, Stream response, PipeReader reader, PipeWriter writer) Start(MessageBody body)
    {
        CanHaveBody = !body.IsEmpty;
        _requestReader.StartAcceptingReads(body);
        _emptyRequestReader.StartAcceptingReads(MessageBody.ZeroContentLengthClose);
        _responseWriter.StartAcceptingWrites();

        if (body.RequestUpgrade)
        {
            // until Upgrade() is called, context.Response.Body should use the normal output stream
            _upgradeableResponse.SetInnerStream(_response);
            _upgradeablePipeWriter.SetInnerPipe(_responseWriter);
            // upgradeable requests should never have a request body
            return (_emptyRequest, _upgradeableResponse, _emptyRequestReader, _upgradeablePipeWriter);
        }
        else if (body.ExtendedConnect)
        {
            // CONNECT requests do not have a request or response body until after accepted,
            // unless it's a 300+ response. We set CanHaveBody to false here since it's only
            // for requests (see IHttpRequestBodyDetectionFeature).
            CanHaveBody = false;
            _connectResponse.SetRequest(body.Context);
            _connectPipeWriter.SetRequest(body.Context);
            return (_emptyRequest, _connectResponse, _emptyRequestReader, _connectPipeWriter);
        }
        else
        {
            return (_request, _response, _requestReader, _responseWriter);
        }
    }

    public Task StopAsync()
    {
        _requestReader.StopAcceptingReads();
        _emptyRequestReader.StopAcceptingReads();
        return _responseWriter.StopAcceptingWritesAsync();
    }

    public void Abort(Exception error)
    {
        _requestReader.Abort(error);
        _emptyRequestReader.Abort(error);
        _responseWriter.Abort();
    }
}