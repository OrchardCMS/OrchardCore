using System.Buffers;
using System.IO.Pipelines;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Media.Services;

internal sealed class ClientDisconnectAwarePipeReader : PipeReader
{
    private const string UnexpectedEndOfRequestContent = "Unexpected end of request content.";

    private readonly PipeReader _inner;
    private bool _disconnected;

    public ClientDisconnectAwarePipeReader(PipeReader inner)
    {
        _inner = inner;
    }

    public override void AdvanceTo(SequencePosition consumed)
    {
        if (!_disconnected)
        {
            _inner.AdvanceTo(consumed);
        }
    }

    public override void AdvanceTo(SequencePosition consumed, SequencePosition examined)
    {
        if (!_disconnected)
        {
            _inner.AdvanceTo(consumed, examined);
        }
    }

    public override void CancelPendingRead() => _inner.CancelPendingRead();

    public override void Complete(Exception exception = null) => _inner.Complete(exception);

    public override async ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (_disconnected)
        {
            return CanceledReadResult();
        }

        try
        {
            return await _inner.ReadAsync(cancellationToken);
        }
        catch (BadHttpRequestException exception) when (IsUnexpectedEndOfRequest(exception))
        {
            _disconnected = true;
            return CanceledReadResult();
        }
    }

    public override bool TryRead(out ReadResult result)
    {
        if (_disconnected)
        {
            result = CanceledReadResult();
            return true;
        }

        try
        {
            return _inner.TryRead(out result);
        }
        catch (BadHttpRequestException exception) when (IsUnexpectedEndOfRequest(exception))
        {
            _disconnected = true;
            result = CanceledReadResult();
            return true;
        }
    }

    internal static bool IsUnexpectedEndOfRequest(BadHttpRequestException exception) =>
        // Kestrel does not expose its internal rejection reason, so the message is the
        // only way to distinguish a truncated body from other malformed requests.
        exception.StatusCode == StatusCodes.Status400BadRequest
        && string.Equals(exception.Message, UnexpectedEndOfRequestContent, StringComparison.Ordinal);

    private static ReadResult CanceledReadResult() =>
        new(ReadOnlySequence<byte>.Empty, isCanceled: true, isCompleted: false);
}
