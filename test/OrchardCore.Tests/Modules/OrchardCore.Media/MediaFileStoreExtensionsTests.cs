using Microsoft.Extensions.Localization;
using OrchardCore.FileStorage;
using OrchardCore.Infrastructure;
using OrchardCore.Media;
using OrchardCore.Media.Core;
using OrchardCore.Media.Events;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public class MediaFileStoreExtensionsTests
{
    [Fact]
    public async Task CreateFileFromStreamAsync_FileEventHandlerCanReplaceStreamBeforeSaving_Succeeds()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var mediaFileStoreMock = new Mock<IMediaFileStore>();
        var inputStream = new MemoryStream("ignored"u8.ToArray());
        var replacementStream = new MemoryStream("hello world"u8.ToArray());
        var handlerMock = new Mock<IFileEventHandler>();

        handlerMock
            .Setup(x => x.CreatingAsync(It.IsAny<FileCreatingContext>(), inputStream, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FileCreatingResult.Success(replacementStream));

        mediaFileStoreMock
            .Setup(x => x.CreateFileFromStreamAsync("test.txt", It.IsAny<Stream>(), false))
            .ReturnsAsync("result")
            .Callback<string, Stream, bool>((_, stream, _) =>
            {
                using var copy = new MemoryStream();
                stream.CopyTo(copy);
                Assert.Equal("hello world"u8.ToArray(), copy.ToArray());
            });

        mediaFileStoreMock
            .Setup(x => x.GetFileInfoAsync("result"))
            .ReturnsAsync(Mock.Of<IFileStoreEntry>());

        var fileCreationService = CreateFileCreationService(handlerMock.Object);

        await mediaFileStoreMock.Object.CreateFileFromStreamAsync(fileCreationService, "test.txt", inputStream, cancellationToken: cancellationToken);
    }

    [Fact]
    public async Task CreateFileFromStreamAsync_FileEventHandlerCanRejectFile_Succeeds()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var mediaFileStoreMock = new Mock<IMediaFileStore>(MockBehavior.Strict);
        var inputStream = new MemoryStream("hello world"u8.ToArray());
        var handlerMock = new Mock<IFileEventHandler>();

        handlerMock
            .Setup(x => x.CreatingAsync(It.IsAny<FileCreatingContext>(), inputStream, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FileCreatingResult.Failed(stream: null, new ResultError
            {
                Message = new LocalizedString("Rejected", "The uploaded file was rejected by the anti-virus scanner."),
            }));

        var fileCreationService = CreateFileCreationService(handlerMock.Object);

        var exception = await Assert.ThrowsAsync<FileStoreException>(() =>
            mediaFileStoreMock.Object.CreateFileFromStreamAsync(fileCreationService, "test.txt", inputStream, cancellationToken: cancellationToken));

        Assert.Equal("The uploaded file was rejected by the anti-virus scanner.", exception.Message);
    }

    [Fact]
    public async Task CreateFileFromStreamAsync_FileEventHandlersRunCreatedAfterTheFileIsStored_Succeeds()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var mediaFileStoreMock = new Mock<IMediaFileStore>();
        var inputStream = new MemoryStream("hello world"u8.ToArray());
        var fileInfoMock = new Mock<IFileStoreEntry>();
        var handlerMock = new Mock<IFileEventHandler>();

        handlerMock
            .Setup(x => x.CreatingAsync(It.IsAny<FileCreatingContext>(), inputStream, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FileCreatingResult.Success(inputStream));

        mediaFileStoreMock
            .Setup(x => x.CreateFileFromStreamAsync("test.txt", inputStream, false))
            .ReturnsAsync("result");

        mediaFileStoreMock
            .Setup(x => x.GetFileInfoAsync("result"))
            .ReturnsAsync(fileInfoMock.Object);

        var fileCreationService = CreateFileCreationService(handlerMock.Object);

        await mediaFileStoreMock.Object.CreateFileFromStreamAsync(fileCreationService, "test.txt", inputStream, cancellationToken: cancellationToken);

        handlerMock.Verify(x => x.CreatedAsync(fileInfoMock.Object, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateFileFromStreamAsync_FileEventHandlersRunBeforeMediaCreatingHandlers_Succeeds()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var fileStoreMock = new Mock<IFileStore>();
        var loggerMock = new Mock<ILogger<DefaultMediaFileStore>>();
        var inputStream = new MemoryStream("hello world"u8.ToArray());
        var calls = new List<string>();
        var fileHandlerMock = new Mock<IFileEventHandler>();
        var mediaHandlerMock = new Mock<IMediaCreatingEventHandler>();

        fileHandlerMock
            .Setup(x => x.CreatingAsync(It.IsAny<FileCreatingContext>(), inputStream, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FileCreatingContext _, Stream stream, CancellationToken _) =>
            {
                calls.Add("file");
                return FileCreatingResult.Success(stream);
            });

        mediaHandlerMock
            .Setup(x => x.MediaCreatingAsync(It.IsAny<MediaCreatingContext>(), inputStream))
            .ReturnsAsync((MediaCreatingContext _, Stream stream) =>
            {
                calls.Add("media");
                return stream;
            });

        fileStoreMock
            .Setup(x => x.CreateFileFromStreamAsync("test.txt", inputStream, false))
            .ReturnsAsync("result");

        fileStoreMock
            .Setup(x => x.GetFileInfoAsync("result"))
            .ReturnsAsync(Mock.Of<IFileStoreEntry>());

        var mediaFileStore = new DefaultMediaFileStore(
            fileStoreMock.Object,
            "",
            "",
            [],
            [mediaHandlerMock.Object],
            loggerMock.Object);

        var fileCreationService = CreateFileCreationService(fileHandlerMock.Object);

        await mediaFileStore.CreateFileFromStreamAsync(fileCreationService, "test.txt", inputStream, cancellationToken: cancellationToken);

        Assert.Equal(["file", "media"], calls);
    }

    [Fact]
    public async Task CreateFileFromStreamAsync_FileEventHandlerRejectionStopsMediaProcessing_Succeeds()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var loggerMock = new Mock<ILogger<DefaultMediaFileStore>>();
        var inputStream = new MemoryStream("hello world"u8.ToArray());
        var fileHandlerMock = new Mock<IFileEventHandler>();
        var mediaHandlerMock = new Mock<IMediaCreatingEventHandler>(MockBehavior.Strict);

        fileHandlerMock
            .Setup(x => x.CreatingAsync(It.IsAny<FileCreatingContext>(), inputStream, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FileCreatingResult.Failed(stream: null, new ResultError
            {
                Message = new LocalizedString("Rejected", "Rejected before media processing."),
            }));

        var mediaFileStore = new DefaultMediaFileStore(
            Mock.Of<IFileStore>(),
            "",
            "",
            [],
            [mediaHandlerMock.Object],
            loggerMock.Object);

        var fileCreationService = CreateFileCreationService(fileHandlerMock.Object);

        var exception = await Assert.ThrowsAsync<FileStoreException>(() =>
            mediaFileStore.CreateFileFromStreamAsync(fileCreationService, "test.txt", inputStream, cancellationToken: cancellationToken));

        Assert.Equal("Rejected before media processing.", exception.Message);
        mediaHandlerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateFileFromStreamAsync_FileEventReplacementStreamLivesThroughMediaProcessingAndStorage_Succeeds()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var fileStoreMock = new Mock<IFileStore>();
        var loggerMock = new Mock<ILogger<DefaultMediaFileStore>>();
        var inputStream = new TrackingStream("ignored"u8.ToArray());
        var scannedStream = new TrackingStream("hello world"u8.ToArray());
        var transformedStream = new TrackingStream("transformed"u8.ToArray());
        var handlerMock = new Mock<IFileEventHandler>();
        var mediaHandlerMock = new Mock<IMediaCreatingEventHandler>();

        handlerMock
            .Setup(x => x.CreatingAsync(It.IsAny<FileCreatingContext>(), inputStream, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FileCreatingResult.Success(scannedStream));

        mediaHandlerMock
            .Setup(x => x.MediaCreatingAsync(It.IsAny<MediaCreatingContext>(), scannedStream))
            .ReturnsAsync((MediaCreatingContext _, Stream stream) =>
            {
                Assert.False(scannedStream.IsDisposed);
                return transformedStream;
            });

        fileStoreMock
            .Setup(x => x.CreateFileFromStreamAsync("test.txt", transformedStream, false))
            .ReturnsAsync("result")
            .Callback<string, Stream, bool>((_, stream, _) =>
            {
                Assert.Same(transformedStream, stream);
                Assert.False(scannedStream.IsDisposed);
                Assert.False(transformedStream.IsDisposed);
            });

        fileStoreMock
            .Setup(x => x.GetFileInfoAsync("result"))
            .ReturnsAsync(Mock.Of<IFileStoreEntry>());

        var mediaFileStore = new DefaultMediaFileStore(
            fileStoreMock.Object,
            "",
            "",
            [],
            [mediaHandlerMock.Object],
            loggerMock.Object);

        var fileCreationService = CreateFileCreationService(handlerMock.Object);

        await mediaFileStore.CreateFileFromStreamAsync(fileCreationService, "test.txt", inputStream, cancellationToken: cancellationToken);

        Assert.False(inputStream.IsDisposed);
        Assert.True(scannedStream.IsDisposed);
        Assert.True(transformedStream.IsDisposed);
    }

    private static FileCreationService CreateFileCreationService(params IFileEventHandler[] handlers)
        => new(handlers);

    private sealed class TrackingStream : MemoryStream
    {
        public TrackingStream(byte[] buffer)
            : base(buffer)
        {
        }

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
