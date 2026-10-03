using Microsoft.Extensions.Logging;
using Moq;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets;

public class SecretManagerTests
{
    private readonly Mock<ILogger<SecretManager>> _loggerMock;

    public SecretManagerTests()
    {
        _loggerMock = new Mock<ILogger<SecretManager>>();
    }

    [Fact]
    public async Task GetSecretAsync_ReturnsSecret_WhenFoundInFirstStore()
    {
        // Arrange
        var expectedSecret = new TextSecret { Text = "test-value" };

        var store1 = new Mock<ISecretStore>();
        store1.Setup(s => s.Name).Returns("Store1");
        store1.Setup(s => s.GetSecretAsync<TextSecret>("test-secret"))
            .ReturnsAsync(expectedSecret);

        var store2 = new Mock<ISecretStore>();
        store2.Setup(s => s.Name).Returns("Store2");

        var manager = new SecretManager([store1.Object, store2.Object], _loggerMock.Object);

        // Act
        var result = await manager.GetSecretAsync<TextSecret>("test-secret");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-value", result.Text);
        store2.Verify(s => s.GetSecretAsync<TextSecret>(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetSecretAsync_SearchesAllStores_WhenNotFoundInFirst()
    {
        // Arrange
        var expectedSecret = new TextSecret { Text = "found-in-second" };

        var store1 = new Mock<ISecretStore>();
        store1.Setup(s => s.Name).Returns("Store1");
        store1.Setup(s => s.GetSecretAsync<TextSecret>("test-secret"))
            .ReturnsAsync((TextSecret)null);

        var store2 = new Mock<ISecretStore>();
        store2.Setup(s => s.Name).Returns("Store2");
        store2.Setup(s => s.GetSecretAsync<TextSecret>("test-secret"))
            .ReturnsAsync(expectedSecret);

        var manager = new SecretManager([store1.Object, store2.Object], _loggerMock.Object);

        // Act
        var result = await manager.GetSecretAsync<TextSecret>("test-secret");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("found-in-second", result.Text);
    }

    [Fact]
    public async Task GetSecretAsync_ReturnsNull_WhenNotFoundInAnyStore()
    {
        // Arrange
        var store1 = new Mock<ISecretStore>();
        store1.Setup(s => s.Name).Returns("Store1");
        store1.Setup(s => s.GetSecretAsync<TextSecret>("test-secret"))
            .ReturnsAsync((TextSecret)null);

        var manager = new SecretManager([store1.Object], _loggerMock.Object);

        // Act
        var result = await manager.GetSecretAsync<TextSecret>("test-secret");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSecretAsync_WithStoreName_ReturnsFromSpecificStore()
    {
        // Arrange
        var secret1 = new TextSecret { Text = "from-store1" };
        var secret2 = new TextSecret { Text = "from-store2" };

        var store1 = new Mock<ISecretStore>();
        store1.Setup(s => s.Name).Returns("Store1");
        store1.Setup(s => s.GetSecretAsync<TextSecret>("test-secret"))
            .ReturnsAsync(secret1);

        var store2 = new Mock<ISecretStore>();
        store2.Setup(s => s.Name).Returns("Store2");
        store2.Setup(s => s.GetSecretAsync<TextSecret>("test-secret"))
            .ReturnsAsync(secret2);

        var manager = new SecretManager([store1.Object, store2.Object], _loggerMock.Object);

        // Act
        var result = await manager.GetSecretAsync<TextSecret>("test-secret", "Store2");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("from-store2", result.Text);
    }

    [Fact]
    public async Task GetSecretAsync_WithStoreName_ReturnsNull_WhenStoreNotFound()
    {
        // Arrange
        var store1 = new Mock<ISecretStore>();
        store1.Setup(s => s.Name).Returns("Store1");

        var manager = new SecretManager([store1.Object], _loggerMock.Object);

        // Act
        var result = await manager.GetSecretAsync<TextSecret>("test-secret", "NonExistentStore");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SaveSecretAsync_SavesTo_DefaultWritableStore()
    {
        // Arrange
        var secret = new TextSecret { Text = "new-secret" };

        var readOnlyStore = new Mock<ISecretStore>();
        readOnlyStore.Setup(s => s.Name).Returns("ReadOnly");
        readOnlyStore.Setup(s => s.IsReadOnly).Returns(true);

        var writableStore = new Mock<ISecretStore>();
        writableStore.Setup(s => s.Name).Returns("Writable");
        writableStore.Setup(s => s.IsReadOnly).Returns(false);

        var manager = new SecretManager([readOnlyStore.Object, writableStore.Object], _loggerMock.Object);

        // Act
        await manager.SaveSecretAsync("test-secret", secret);

        // Assert
        writableStore.Verify(s => s.SaveSecretAsync("test-secret", secret, It.IsAny<SecretSaveOptions>()), Times.Once);
        readOnlyStore.Verify(s => s.SaveSecretAsync(It.IsAny<string>(), It.IsAny<TextSecret>(), It.IsAny<SecretSaveOptions>()), Times.Never);
    }

    [Fact]
    public async Task SaveSecretAsync_ThrowsException_WhenNoWritableStore()
    {
        // Arrange
        var secret = new TextSecret { Text = "new-secret" };

        var readOnlyStore = new Mock<ISecretStore>();
        readOnlyStore.Setup(s => s.Name).Returns("ReadOnly");
        readOnlyStore.Setup(s => s.IsReadOnly).Returns(true);

        var manager = new SecretManager([readOnlyStore.Object], _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => manager.SaveSecretAsync("test-secret", secret));
    }

    [Fact]
    public async Task SaveSecretAsync_WithStoreName_SavesTo_SpecificStore()
    {
        // Arrange
        var secret = new TextSecret { Text = "new-secret" };

        var store1 = new Mock<ISecretStore>();
        store1.Setup(s => s.Name).Returns("Store1");
        store1.Setup(s => s.IsReadOnly).Returns(false);

        var store2 = new Mock<ISecretStore>();
        store2.Setup(s => s.Name).Returns("Store2");
        store2.Setup(s => s.IsReadOnly).Returns(false);

        var manager = new SecretManager([store1.Object, store2.Object], _loggerMock.Object);

        // Act
        await manager.SaveSecretAsync("test-secret", secret, "Store2");

        // Assert
        store2.Verify(s => s.SaveSecretAsync("test-secret", secret, It.IsAny<SecretSaveOptions>()), Times.Once);
        store1.Verify(s => s.SaveSecretAsync(It.IsAny<string>(), It.IsAny<TextSecret>(), It.IsAny<SecretSaveOptions>()), Times.Never);
    }

    [Fact]
    public async Task SaveSecretAsync_WithStoreName_ThrowsException_WhenStoreIsReadOnly()
    {
        // Arrange
        var secret = new TextSecret { Text = "new-secret" };

        var readOnlyStore = new Mock<ISecretStore>();
        readOnlyStore.Setup(s => s.Name).Returns("ReadOnly");
        readOnlyStore.Setup(s => s.IsReadOnly).Returns(true);

        var manager = new SecretManager([readOnlyStore.Object], _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => manager.SaveSecretAsync("test-secret", secret, "ReadOnly"));
    }

    [Fact]
    public async Task RemoveSecretAsync_RemovesFromAllWritableStores()
    {
        // Arrange
        var readOnlyStore = new Mock<ISecretStore>();
        readOnlyStore.Setup(s => s.Name).Returns("ReadOnly");
        readOnlyStore.Setup(s => s.IsReadOnly).Returns(true);

        var writableStore1 = new Mock<ISecretStore>();
        writableStore1.Setup(s => s.Name).Returns("Writable1");
        writableStore1.Setup(s => s.IsReadOnly).Returns(false);

        var writableStore2 = new Mock<ISecretStore>();
        writableStore2.Setup(s => s.Name).Returns("Writable2");
        writableStore2.Setup(s => s.IsReadOnly).Returns(false);

        var manager = new SecretManager([readOnlyStore.Object, writableStore1.Object, writableStore2.Object], _loggerMock.Object);

        // Act
        await manager.RemoveSecretAsync("test-secret");

        // Assert
        writableStore1.Verify(s => s.RemoveSecretAsync("test-secret"), Times.Once);
        writableStore2.Verify(s => s.RemoveSecretAsync("test-secret"), Times.Once);
        readOnlyStore.Verify(s => s.RemoveSecretAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetSecretInfosAsync_ReturnsInfosFromAllStores()
    {
        // Arrange
        var store1Infos = new List<SecretInfo>
        {
            new() { Name = "secret1", Store = "Store1" },
            new() { Name = "secret2", Store = "Store1" },
        };

        var store2Infos = new List<SecretInfo>
        {
            new() { Name = "secret3", Store = "Store2" },
        };

        var store1 = new Mock<ISecretStore>();
        store1.Setup(s => s.Name).Returns("Store1");
        store1.Setup(s => s.GetSecretInfosAsync()).ReturnsAsync(store1Infos);

        var store2 = new Mock<ISecretStore>();
        store2.Setup(s => s.Name).Returns("Store2");
        store2.Setup(s => s.GetSecretInfosAsync()).ReturnsAsync(store2Infos);

        var manager = new SecretManager([store1.Object, store2.Object], _loggerMock.Object);

        // Act
        var result = (await manager.GetSecretInfosAsync()).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, i => i.Name == "secret1");
        Assert.Contains(result, i => i.Name == "secret2");
        Assert.Contains(result, i => i.Name == "secret3");
    }

    [Fact]
    public void GetStores_ReturnsAllConfiguredStores()
    {
        // Arrange
        var store1 = new Mock<ISecretStore>();
        store1.Setup(s => s.Name).Returns("Store1");

        var store2 = new Mock<ISecretStore>();
        store2.Setup(s => s.Name).Returns("Store2");

        var manager = new SecretManager([store1.Object, store2.Object], _loggerMock.Object);

        // Act
        var stores = manager.GetStores().ToList();

        // Assert
        Assert.Equal(2, stores.Count);
    }
}

