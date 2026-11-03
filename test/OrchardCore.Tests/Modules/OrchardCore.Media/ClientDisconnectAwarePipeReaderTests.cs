using System.Buffers;
using System.IO.Pipelines;
using Microsoft.AspNetCore.Http;
using OrchardCore.Media.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public sealed class ClientDisconnectAwarePipeReaderTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Read_CanceledEmptyBufferWithRecycledSegment_ReturnsSafeCanceledRead(bool useTryRead)
    {
        var inner = new CanceledPipeReader(recycleSegment: true);
        var reader = new ClientDisconnectAwarePipeReader(inner);

        ReadResult result;
        if (useTryRead)
        {
            Assert.True(reader.TryRead(out result));
        }
        else
        {
            result = await reader.ReadAsync(TestContext.Current.CancellationToken);
        }

        Assert.True(result.IsCanceled);
        Assert.False(result.IsCompleted);
        Assert.True(result.Buffer.IsEmpty);
        foreach (var segment in result.Buffer)
        {
            Assert.True(segment.IsEmpty);
        }

        reader.AdvanceTo(result.Buffer.End);
        reader.AdvanceTo(result.Buffer.End, result.Buffer.End);

        var next = await reader.ReadAsync(TestContext.Current.CancellationToken);
        Assert.True(next.IsCanceled);
        Assert.True(next.Buffer.IsEmpty);
        Assert.Equal(1, inner.ReadCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Read_CanceledBufferWithData_PreservesPartialData(bool useTryRead)
    {
        var inner = new CanceledPipeReader(recycleSegment: false);
        var reader = new ClientDisconnectAwarePipeReader(inner);

        ReadResult result;
        if (useTryRead)
        {
            Assert.True(reader.TryRead(out result));
        }
        else
        {
            result = await reader.ReadAsync(TestContext.Current.CancellationToken);
        }

        Assert.True(result.IsCanceled);
        Assert.Equal("partial upload"u8.ToArray(), result.Buffer.ToArray());
    }

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

    private sealed class CanceledPipeReader : PipeReader
    {
        private readonly ReadOnlySequence<byte> _buffer;

        public CanceledPipeReader(bool recycleSegment)
        {
            var segment = new RecyclableSegment("partial upload"u8.ToArray());
            var start = recycleSegment ? segment.Memory.Length : 0;
            _buffer = new ReadOnlySequence<byte>(segment, start, segment, segment.Memory.Length);

            if (recycleSegment)
            {
                segment.Recycle();
            }
        }

        public int ReadCount { get; private set; }

        public override void AdvanceTo(SequencePosition consumed) =>
            throw new InvalidOperationException("A disconnected read must not be advanced.");

        public override void AdvanceTo(SequencePosition consumed, SequencePosition examined) =>
            throw new InvalidOperationException("A disconnected read must not be advanced.");

        public override void CancelPendingRead()
        {
        }

        public override void Complete(Exception exception = null)
        {
        }

        public override ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
        {
            ReadCount++;
            return ValueTask.FromResult(new ReadResult(_buffer, isCanceled: true, isCompleted: false));
        }

        public override bool TryRead(out ReadResult result)
        {
            ReadCount++;
            result = new ReadResult(_buffer, isCanceled: true, isCompleted: false);
            return true;
        }
    }

    private sealed class RecyclableSegment : ReadOnlySequenceSegment<byte>
    {
        public RecyclableSegment(byte[] data)
        {
            Memory = data;
        }

        public void Recycle() => Memory = ReadOnlyMemory<byte>.Empty;
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
