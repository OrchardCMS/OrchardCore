using OrchardCore.FileStorage;
using OrchardCore.Infrastructure;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public class FileCreationServiceTests
{
    [Fact]
    public async Task CreateAsync_DisposesReplacementStream_WhenLaterHandlerThrows()
    {
        var originalStream = new TrackingStream();
        var replacementStream = new TrackingStream();

        var firstHandler = new Mock<IFileEventHandler>();
        firstHandler
            .Setup(x => x.CreatingAsync(It.IsAny<FileCreatingContext>(), originalStream, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FileCreatingResult.Success(replacementStream));

        var secondHandler = new Mock<IFileEventHandler>();
        secondHandler
            .Setup(x => x.CreatingAsync(It.IsAny<FileCreatingContext>(), replacementStream, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Handler failure."));

        var service = new FileCreationService([firstHandler.Object, secondHandler.Object]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new FileCreatingContext("file.txt"), originalStream, TestContext.Current.CancellationToken));

        Assert.True(replacementStream.IsDisposed);
        Assert.True(originalStream.IsDisposed);
    }

    [Fact]
    public async Task CreateAsync_ReturnedFailedResult_DisposesReplacementStream()
    {
        var originalStream = new TrackingStream();
        var replacementStream = new TrackingStream();

        var firstHandler = new Mock<IFileEventHandler>();
        firstHandler
            .Setup(x => x.CreatingAsync(It.IsAny<FileCreatingContext>(), originalStream, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FileCreatingResult.Success(replacementStream));

        var secondHandler = new Mock<IFileEventHandler>();
        secondHandler
            .Setup(x => x.CreatingAsync(It.IsAny<FileCreatingContext>(), replacementStream, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FileCreatingResult.Failed(replacementStream, new ResultError
            {
                Message = new LocalizedString("Rejected", "The file was rejected."),
            }));

        var service = new FileCreationService([firstHandler.Object, secondHandler.Object]);

        await using (var result = await service.CreateAsync(new FileCreatingContext("file.txt"), originalStream, TestContext.Current.CancellationToken))
        {
            Assert.False(result.Succeeded);
            Assert.Same(replacementStream, result.Stream);
            Assert.False(replacementStream.IsDisposed);
        }

        Assert.True(replacementStream.IsDisposed);
        Assert.True(originalStream.IsDisposed);
    }

    [Fact]
    public async Task CreateAsync_LeavesOriginalStreamOpen_WhenRequested()
    {
        var originalStream = new TrackingStream();
        var service = new FileCreationService([]);

        await using (var result = await service.CreateAsync(
            new FileCreatingContext("file.txt"),
            originalStream,
            leaveOpen: true,
            cancellationToken: TestContext.Current.CancellationToken))
        {
            Assert.True(result.Succeeded);
            Assert.Same(originalStream, result.Stream);
        }

        Assert.False(originalStream.IsDisposed);
    }

    private sealed class TrackingStream : MemoryStream
    {
        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            IsDisposed = true;
            await base.DisposeAsync();
        }
    }
}
