using System.Buffers;
using System.IO.Pipelines;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Media;
using OrchardCore.Media.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public sealed class DiskTusTempStoreTests : IDisposable
{
    private readonly string _tempPath = Path.Combine(
        Path.GetTempPath(),
        nameof(DiskTusTempStoreTests),
        Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task AppendDataAsync_CanceledPipeReader_PersistsPartialData()
    {
        var store = new DiskTusTempStore(
            Options.Create(new MediaOptions { TusTempPath = _tempPath }),
            new ShellSettings { VersionId = "tenant" },
            NullLogger<DiskTusTempStore>.Instance);
        var data = "partial upload"u8.ToArray();
        var reader = new CanceledPipeReader(data);
        var cancellationToken = TestContext.Current.CancellationToken;

        await store.CreateFileAsync("file", cancellationToken);
        var bytesWritten = await store.AppendDataAsync("file", reader, 0, cancellationToken);

        Assert.Equal(data.Length, bytesWritten);

        using var stream = store.OpenReadStream("file");
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        Assert.Equal(data, memoryStream.ToArray());
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempPath))
        {
            Directory.Delete(_tempPath, recursive: true);
        }
    }

    private sealed class CanceledPipeReader : PipeReader
    {
        private readonly byte[] _data;
        private bool _read;

        public CanceledPipeReader(byte[] data)
        {
            _data = data;
        }

        public override void AdvanceTo(SequencePosition consumed)
        {
        }

        public override void AdvanceTo(SequencePosition consumed, SequencePosition examined)
        {
        }

        public override void CancelPendingRead()
        {
        }

        public override void Complete(Exception exception = null)
        {
        }

        public override ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
        {
            Assert.False(_read);
            _read = true;

            return ValueTask.FromResult(
                new ReadResult(new ReadOnlySequence<byte>(_data), isCanceled: true, isCompleted: false));
        }

        public override bool TryRead(out ReadResult result)
        {
            result = default;
            return false;
        }
    }
}
