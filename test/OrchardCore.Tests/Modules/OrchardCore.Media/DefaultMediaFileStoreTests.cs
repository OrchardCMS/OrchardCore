using Microsoft.Extensions.Localization;
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
    public void MapPathToPublicUrl_ReturnsUrlEncodedPath(string path, string expected)
    {
        var store = new DefaultMediaFileStore(
            Mock.Of<IFileStore>(),
            "/media",
            "",
            [],
            [],
            CreateFileEventService(),
            Mock.Of<ILogger<DefaultMediaFileStore>>());

        var result = store.MapPathToPublicUrl(path);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("https://cdn.example.com", "foo bar.jpg", "https://cdn.example.com/media/foo%20bar.jpg")]
    [InlineData("https://cdn.example.com", "bàr.jpeg", "https://cdn.example.com/media/b%C3%A0r.jpeg")]
    public void MapPathToPublicUrl_WithCdnBaseUrl_ReturnsUrlEncodedPath(string cdnBaseUrl, string path, string expected)
    {
        var store = new DefaultMediaFileStore(
            Mock.Of<IFileStore>(),
            "/media",
            cdnBaseUrl,
            [],
            [],
            CreateFileEventService(),
            Mock.Of<ILogger<DefaultMediaFileStore>>());

        var result = store.MapPathToPublicUrl(path);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task CreateFileFromStreamAsync_NoHandlers_CallsFileStoreDirectly()
    {
        // Arrange
        var fileStoreMock = new Mock<IFileStore>();
        var loggerMock = new Mock<ILogger<DefaultMediaFileStore>>();
        var inputStream = new MemoryStream();
        fileStoreMock
            .Setup(x => x.CreateFileFromStreamAsync("test.txt", inputStream, false))
            .ReturnsAsync("result");

        fileStoreMock
            .Setup(x => x.GetFileInfoAsync("result"))
            .ReturnsAsync(Mock.Of<IFileStoreEntry>());

        var store = new DefaultMediaFileStore(
            fileStoreMock.Object,
            "",
            "",
            new List<IMediaEventHandler>(),
            new List<IMediaCreatingEventHandler>(),
            CreateFileEventService(),
            loggerMock.Object);

        // Act
        var result = await store.CreateFileFromStreamAsync("test.txt", inputStream);

        // Assert
        Assert.Equal("result", result);
        fileStoreMock.Verify(x => x.CreateFileFromStreamAsync("test.txt", inputStream, false), Times.Once);
    }

    [Fact]
    public async Task CreateFileFromStreamAsync_HandlerReturnsInputStream_PassesInputStreamToFileStore()
    {
        // Arrange
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
            new List<IMediaEventHandler>(),
            new[] { handlerMock.Object },
            CreateFileEventService(),
            loggerMock.Object);

        // Act
        var result = await store.CreateFileFromStreamAsync("test.txt", inputStream);

        // Assert
        Assert.Equal("result", result);
        fileStoreMock.Verify(x => x.CreateFileFromStreamAsync("test.txt", inputStream, false), Times.Once);
    }

    [Fact]
    public async Task CreateFileFromStreamAsync_HandlersCreateNewStreams_PassesCorrectStreams()
    {
        // Arrange
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

        fileStoreMock
            .Setup(x => x.GetFileInfoAsync("result"))
            .ReturnsAsync(Mock.Of<IFileStoreEntry>());

        var store = new DefaultMediaFileStore(
            fileStoreMock.Object,
            "",
            "",
            new List<IMediaEventHandler>(),
            new[] { handler1.Object, handler2.Object },
            CreateFileEventService(),
            loggerMock.Object);

        // Act
        var result = await store.CreateFileFromStreamAsync("test.txt", inputStream);

        // Assert
        Assert.Equal("result", result);
        handler1.Verify(x => x.MediaCreatingAsync(It.IsAny<MediaCreatingContext>(), inputStream), Times.Once);
        handler2.Verify(x => x.MediaCreatingAsync(It.IsAny<MediaCreatingContext>(), stream1), Times.Once);
        fileStoreMock.Verify(x => x.CreateFileFromStreamAsync("test.txt", stream2, false), Times.Once);
    }

    [Fact]
    public async Task CreateFileFromStreamAsync_ValidateAvailableStorageAsync()
    {
        // Arrange
        var fileStoreMock = new Mock<IFileStore>();
        var loggerMock = new Mock<ILogger<DefaultMediaFileStore>>();
        var stream1 = new MemoryStream(new byte[100]);
        var stream2 = new MemoryStream(new byte[200]);
        var handler = new Mock<IMediaEventHandler>();

        handler
            .Setup(x => x.MediaPermittedStorageAsync(It.IsAny<MediaPermittedStorageContext>()))
            .Returns<MediaPermittedStorageContext>(context =>
            {
                context.Constrain(150);
                return Task.CompletedTask;
            });

        fileStoreMock
            .Setup(x => x.CreateFileFromStreamAsync("test.txt", stream2, false))
            .ReturnsAsync("result");

        var store = new DefaultMediaFileStore(
            fileStoreMock.Object,
            "",
            "",
            [handler.Object],
            [],
            CreateFileEventService(),
            loggerMock.Object);

        // Act
        await store.CreateFileFromStreamAsync("test1.txt", stream1);
        var exception = await Assert.ThrowsAsync<FileStoreException>(() =>
            store.CreateFileFromStreamAsync("test2.txt", stream2));

        // Assert
        const string expectedMessage =
            "You tried to upload a file that requires 200 B of storage space, but only 150 B is available. Try " +
            "uploading a file that fits the available space, or delete some unnecessary files.";
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public async Task CreateFileFromStreamAsync_FileEventHandlerCanReplaceStreamBeforeSaving()
    {
        var fileStoreMock = new Mock<IFileStore>();
        var loggerMock = new Mock<ILogger<DefaultMediaFileStore>>();
        var inputStream = new MemoryStream("ignored"u8.ToArray());
        var replacementStream = new MemoryStream("hello world"u8.ToArray());
        var handlerMock = new Mock<IFileEventHandler>();

        handlerMock
            .Setup(x => x.CreatingAsync(It.IsAny<FileCreatingContext>(), inputStream, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FileCreatingResult.Success(replacementStream));

        fileStoreMock
            .Setup(x => x.CreateFileFromStreamAsync("test.txt", It.IsAny<Stream>(), false))
            .ReturnsAsync("result")
            .Callback<string, Stream, bool>((_, stream, _) =>
            {
                using var copy = new MemoryStream();
                stream.CopyTo(copy);
                Assert.Equal("hello world"u8.ToArray(), copy.ToArray());
            });

        fileStoreMock
            .Setup(x => x.GetFileInfoAsync("result"))
            .ReturnsAsync(Mock.Of<IFileStoreEntry>());

        var store = new DefaultMediaFileStore(
            fileStoreMock.Object,
            "",
            "",
            [],
            [],
            CreateFileEventService(handlerMock.Object),
            loggerMock.Object);

        await store.CreateFileFromStreamAsync("test.txt", inputStream);
    }

    [Fact]
    public async Task CreateFileFromStreamAsync_FileEventHandlerCanRejectFile()
    {
        var fileStoreMock = new Mock<IFileStore>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<DefaultMediaFileStore>>();
        var inputStream = new MemoryStream("hello world"u8.ToArray());
        var handlerMock = new Mock<IFileEventHandler>();

        handlerMock
            .Setup(x => x.CreatingAsync(It.IsAny<FileCreatingContext>(), inputStream, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FileCreatingResult.Failed(stream: null,
                new ResultError
                {
                    Message = new LocalizedString("Rejected", "The uploaded file was rejected by the anti-virus scanner."),
                }));

        var store = new DefaultMediaFileStore(
            fileStoreMock.Object,
            "",
            "",
            [],
            [],
            CreateFileEventService(handlerMock.Object),
            loggerMock.Object);

        var exception = await Assert.ThrowsAsync<FileStoreException>(() =>
            store.CreateFileFromStreamAsync("test.txt", inputStream));

        Assert.Equal("The uploaded file was rejected by the anti-virus scanner.", exception.Message);
    }

    [Fact]
    public async Task CreateFileFromStreamAsync_FileEventHandlersRunCreatedAfterTheFileIsStored()
    {
        var fileStoreMock = new Mock<IFileStore>();
        var loggerMock = new Mock<ILogger<DefaultMediaFileStore>>();
        var inputStream = new MemoryStream("hello world"u8.ToArray());
        var fileInfoMock = new Mock<IFileStoreEntry>();
        var handlerMock = new Mock<IFileEventHandler>();

        handlerMock
            .Setup(x => x.CreatingAsync(It.IsAny<FileCreatingContext>(), inputStream, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FileCreatingResult.Success(inputStream));

        fileStoreMock
            .Setup(x => x.CreateFileFromStreamAsync("test.txt", inputStream, false))
            .ReturnsAsync("result");

        fileStoreMock
            .Setup(x => x.GetFileInfoAsync("result"))
            .ReturnsAsync(fileInfoMock.Object);

        var store = new DefaultMediaFileStore(
            fileStoreMock.Object,
            "",
            "",
            [],
            [],
            CreateFileEventService(handlerMock.Object),
            loggerMock.Object);

        await store.CreateFileFromStreamAsync("test.txt", inputStream);

        handlerMock.Verify(x => x.CreatedAsync(fileInfoMock.Object, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static FileEventService CreateFileEventService(params IFileEventHandler[] handlers)
        => new(handlers);
}
