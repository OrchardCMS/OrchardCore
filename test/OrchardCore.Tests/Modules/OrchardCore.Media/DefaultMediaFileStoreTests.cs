using OrchardCore.FileStorage;
using OrchardCore.Infrastructure;
using OrchardCore.Media.Core;
using OrchardCore.Media.Events;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public class DefaultMediaFileStoreTests
{
    [Theory]
    [InlineData("foo bar.jpg", "/media/foo%20bar.jpg")]
    [InlineData("my folder/foo bar.jpg", "/media/my%20folder/foo%20bar.jpg")]
    [InlineData("bàr.jpeg", "/media/b%C3%A0r.jpeg")]
    [InlineData("日本語.jpg", "/media/%E6%97%A5%E6%9C%AC%E8%AA%9E.jpg")]
    [InlineData("simple.jpg", "/media/simple.jpg")]
    [InlineData("sub/dir/file.jpg", "/media/sub/dir/file.jpg")]
    public void MapPathToPublicUrl_Default_ReturnsUrlEncodedPath(string path, string expected)
    {
        var store = new DefaultMediaFileStore(
            Mock.Of<IFileStore>(),
            "/media",
            "",
            [],
            [],
            Mock.Of<ILogger<DefaultMediaFileStore>>());

        var result = store.MapPathToPublicUrl(path);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("https://cdn.example.com", "foo bar.jpg", "https://cdn.example.com/media/foo%20bar.jpg")]
    [InlineData("https://cdn.example.com", "bàr.jpeg", "https://cdn.example.com/media/b%C3%A0r.jpeg")]
    public void MapPathToPublicUrl_CdnBaseUrl_ReturnsUrlEncodedPath(string cdnBaseUrl, string path, string expected)
    {
        var store = new DefaultMediaFileStore(
            Mock.Of<IFileStore>(),
            "/media",
            cdnBaseUrl,
            [],
            [],
            Mock.Of<ILogger<DefaultMediaFileStore>>());

        var result = store.MapPathToPublicUrl(path);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task CreateFileFromStreamAsync_NoHandlers_CallsFileStoreDirectly()
    {
        var fileStoreMock = new Mock<IFileStore>();
        var loggerMock = new Mock<ILogger<DefaultMediaFileStore>>();
        var inputStream = new MemoryStream();

        fileStoreMock
            .Setup(x => x.CreateFileFromStreamAsync("test.txt", inputStream, false))
            .ReturnsAsync("result");

        var store = new DefaultMediaFileStore(
            fileStoreMock.Object,
            "",
            "",
            [],
            [],
            loggerMock.Object);

        var result = await store.CreateFileFromStreamAsync("test.txt", inputStream);

        Assert.Equal("result", result);
        fileStoreMock.Verify(x => x.CreateFileFromStreamAsync("test.txt", inputStream, false), Times.Once);
    }

    [Fact]
    public async Task CreateFileFromStreamAsync_HandlerReturnsInputStream_PassesesesInputStreamToFileStore()
    {
        var fileStoreMock = new Mock<IFileStore>();
        var loggerMock = new Mock<ILogger<DefaultMediaFileStore>>();
        var inputStream = new MemoryStream();
        var handlerMock = new Mock<IMediaCreatingEventHandler>();

        handlerMock
            .Setup(x => x.MediaCreatingAsync(It.IsAny<MediaCreatingContext>(), inputStream))
            .ReturnsAsync(inputStream);

        fileStoreMock
            .Setup(x => x.CreateFileFromStreamAsync("test.txt", inputStream, false))
            .ReturnsAsync("result");

        var store = new DefaultMediaFileStore(
            fileStoreMock.Object,
            "",
            "",
            [],
            [handlerMock.Object],
            loggerMock.Object);

        var result = await store.CreateFileFromStreamAsync("test.txt", inputStream);

        Assert.Equal("result", result);
        fileStoreMock.Verify(x => x.CreateFileFromStreamAsync("test.txt", inputStream, false), Times.Once);
    }

    [Fact]
    public async Task CreateFileFromStreamAsync_HandlersCreateNewStreams_PassesesesCorrectStreams()
    {
        var fileStoreMock = new Mock<IFileStore>();
        var loggerMock = new Mock<ILogger<DefaultMediaFileStore>>();
        var inputStream = new MemoryStream();
        var stream1 = new MemoryStream();
        var stream2 = new MemoryStream();

        var handler1 = new Mock<IMediaCreatingEventHandler>();
        var handler2 = new Mock<IMediaCreatingEventHandler>();

        handler1
            .Setup(x => x.MediaCreatingAsync(It.IsAny<MediaCreatingContext>(), inputStream))
            .ReturnsAsync(stream1);
        handler2
            .Setup(x => x.MediaCreatingAsync(It.IsAny<MediaCreatingContext>(), stream1))
            .ReturnsAsync(stream2);

        fileStoreMock
            .Setup(x => x.CreateFileFromStreamAsync("test.txt", stream2, false))
            .ReturnsAsync("result");

        var store = new DefaultMediaFileStore(
            fileStoreMock.Object,
            "",
            "",
            [],
            [handler1.Object, handler2.Object],
            loggerMock.Object);

        var result = await store.CreateFileFromStreamAsync("test.txt", inputStream);

        Assert.Equal("result", result);
        handler1.Verify(x => x.MediaCreatingAsync(It.IsAny<MediaCreatingContext>(), inputStream), Times.Once);
        handler2.Verify(x => x.MediaCreatingAsync(It.IsAny<MediaCreatingContext>(), stream1), Times.Once);
        fileStoreMock.Verify(x => x.CreateFileFromStreamAsync("test.txt", stream2, false), Times.Once);
    }

    [Fact]
    public async Task CreateFileFromStreamAsync_ValidateAvailableStorageAsync_UsesFinalMediaStreamLength()
    {
        var fileStoreMock = new Mock<IFileStore>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<DefaultMediaFileStore>>();
        var inputStream = new MemoryStream(new byte[100]);
        var transformedStream = new MemoryStream(new byte[200]);
        var mediaEventHandler = new Mock<IMediaEventHandler>();
        var mediaHandlerMock = new Mock<IMediaCreatingEventHandler>();

        mediaEventHandler
            .Setup(x => x.MediaPermittedStorageAsync(It.IsAny<MediaPermittedStorageContext>()))
            .Returns<MediaPermittedStorageContext>(context =>
            {
                context.Constrain(150);
                return Task.CompletedTask;
            });

        mediaHandlerMock
            .Setup(x => x.MediaCreatingAsync(It.IsAny<MediaCreatingContext>(), inputStream))
            .ReturnsAsync(transformedStream);

        fileStoreMock
            .Setup(x => x.GetPermittedStorageAsync())
            .ReturnsAsync((long?)null);

        var store = new DefaultMediaFileStore(
            fileStoreMock.Object,
            "",
            "",
            [mediaEventHandler.Object],
            [mediaHandlerMock.Object],
            loggerMock.Object);

        var exception = await Assert.ThrowsAsync<FileStoreException>(() =>
            store.CreateFileFromStreamAsync("test.txt", inputStream));

        const string expectedMessage =
            "You tried to upload a file that requires 200 B of storage space, but only 150 B is available. Try " +
            "uploading a file that fits the available space, or delete some unnecessary files.";
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public async Task CreateFileFromStreamAsync_MediaReplacementStreamLivesThroughStorage_Succeeds()
    {
        var fileStoreMock = new Mock<IFileStore>();
        var loggerMock = new Mock<ILogger<DefaultMediaFileStore>>();
        var inputStream = new TrackingStream("ignored"u8.ToArray());
        var transformedStream = new TrackingStream("transformed"u8.ToArray());
        var mediaHandlerMock = new Mock<IMediaCreatingEventHandler>();

        mediaHandlerMock
            .Setup(x => x.MediaCreatingAsync(It.IsAny<MediaCreatingContext>(), inputStream))
            .ReturnsAsync((MediaCreatingContext _, Stream stream) =>
            {
                Assert.False(inputStream.IsDisposed);
                return transformedStream;
            });

        fileStoreMock
            .Setup(x => x.CreateFileFromStreamAsync("test.txt", transformedStream, false))
            .ReturnsAsync("result")
            .Callback<string, Stream, bool>((_, stream, _) =>
            {
                Assert.Same(transformedStream, stream);
                Assert.False(inputStream.IsDisposed);
                Assert.False(transformedStream.IsDisposed);
            });

        var store = new DefaultMediaFileStore(
            fileStoreMock.Object,
            "",
            "",
            [],
            [mediaHandlerMock.Object],
            loggerMock.Object);

        await store.CreateFileFromStreamAsync("test.txt", inputStream);

        Assert.False(inputStream.IsDisposed);
        Assert.True(transformedStream.IsDisposed);
    }

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
