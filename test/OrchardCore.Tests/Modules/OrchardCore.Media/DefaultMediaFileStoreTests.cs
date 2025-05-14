using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Moq;
using Xunit;
using OrchardCore.Media.Core;
using OrchardCore.FileStorage;
using OrchardCore.Media.Events;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public class DefaultMediaFileStoreTests
{
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

        var store = new DefaultMediaFileStore(
            fileStoreMock.Object,
            "",
            "",
            new List<IMediaEventHandler>(),
            new List<IMediaCreatingEventHandler>(),
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

        var store = new DefaultMediaFileStore(
            fileStoreMock.Object,
            "",
            "",
            new List<IMediaEventHandler>(),
            new[] { handler1.Object, handler2.Object },
            loggerMock.Object);

        // Act
        var result = await store.CreateFileFromStreamAsync("test.txt", inputStream);

        // Assert
        Assert.Equal("result", result);
        handler1.Verify(x => x.MediaCreatingAsync(It.IsAny<MediaCreatingContext>(), inputStream), Times.Once);
        handler2.Verify(x => x.MediaCreatingAsync(It.IsAny<MediaCreatingContext>(), stream1), Times.Once);
        fileStoreMock.Verify(x => x.CreateFileFromStreamAsync("test.txt", stream2, false), Times.Once);
    }
}

