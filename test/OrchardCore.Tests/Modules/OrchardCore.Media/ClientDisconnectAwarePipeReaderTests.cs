using System.Buffers;
using System.IO.Pipelines;
using Microsoft.AspNetCore.Http;
using OrchardCore.Media.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public sealed class ClientDisconnectAwarePipeReaderTests
{
    [Fact]
    public async Task ReadAsync_UnexpectedEndOfRequest_ReturnsCanceledRead()
    {
        var inner = new ThrowingPipeReader(
            new BadHttpRequestException("Unexpected end of request content."));
        var reader = new ClientDisconnectAwarePipeReader(inner);

        var result = await reader.ReadAsync(TestContext.Current.CancellationToken);
        reader.AdvanceTo(result.Buffer.End);

        Assert.True(result.IsCanceled);
        Assert.False(result.IsCompleted);
        Assert.Equal(1, inner.ReadCount);

        result = await reader.ReadAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsCanceled);
        Assert.Equal(1, inner.ReadCount);
    }

    [Fact]
    public async Task ReadAsync_OtherBadRequest_Throws()
    {
        var exception = new BadHttpRequestException("Invalid chunk terminator.");
        var reader = new ClientDisconnectAwarePipeReader(new ThrowingPipeReader(exception));

        var actual = await Assert.ThrowsAsync<BadHttpRequestException>(
            async () => await reader.ReadAsync(TestContext.Current.CancellationToken));

        Assert.Same(exception, actual);
    }

    private sealed class ThrowingPipeReader : PipeReader
    {
        private readonly Exception _exception;

        public ThrowingPipeReader(Exception exception)
        {
            _exception = exception;
        }

        public int ReadCount { get; private set; }

        public override void AdvanceTo(SequencePosition consumed)
        {
            throw new InvalidOperationException("A failed read must not be advanced.");
        }

        public override void AdvanceTo(SequencePosition consumed, SequencePosition examined)
        {
            throw new InvalidOperationException("A failed read must not be advanced.");
        }

        public override void CancelPendingRead()
        {
        }

        public override void Complete(Exception exception = null)
        {
        }

        public override ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
        {
            ReadCount++;
            return ValueTask.FromException<ReadResult>(_exception);
        }

        public override bool TryRead(out ReadResult result)
        {
            throw _exception;
        }
    }
}
