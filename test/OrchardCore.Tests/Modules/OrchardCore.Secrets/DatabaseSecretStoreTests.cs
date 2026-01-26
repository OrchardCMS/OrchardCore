using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Moq;
using OrchardCore.Documents;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Models;
using OrchardCore.Secrets.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets;

public class DatabaseSecretStoreTests
{
    private readonly Mock<IDocumentManager<SecretsDocument>> _documentManagerMock;
    private readonly Mock<IDataProtectionProvider> _dataProtectionProviderMock;
    private readonly Mock<IDataProtector> _dataProtectorMock;
    private readonly Mock<ILogger<DatabaseSecretStore>> _loggerMock;
    private readonly DatabaseSecretStore _store;

    public DatabaseSecretStoreTests()
    {
        _documentManagerMock = new Mock<IDocumentManager<SecretsDocument>>();
        _dataProtectionProviderMock = new Mock<IDataProtectionProvider>();
        _dataProtectorMock = new Mock<IDataProtector>();
        _loggerMock = new Mock<ILogger<DatabaseSecretStore>>();

        _dataProtectionProviderMock
            .Setup(p => p.CreateProtector(It.IsAny<string>()))
            .Returns(_dataProtectorMock.Object);

        _store = new DatabaseSecretStore(
            _documentManagerMock.Object,
            _dataProtectionProviderMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void Name_ReturnsDatabase()
    {
        Assert.Equal("Database", _store.Name);
    }

    [Fact]
    public void IsReadOnly_ReturnsFalse()
    {
        Assert.False(_store.IsReadOnly);
    }

    [Fact]
    public async Task GetSecretAsync_ReturnsNull_WhenSecretNotFound()
    {
        // Arrange
        var document = new SecretsDocument();
        _documentManagerMock
            .Setup(m => m.GetOrCreateImmutableAsync())
            .ReturnsAsync(document);

        // Act
        var result = await _store.GetSecretAsync<TextSecret>("non-existent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSecretAsync_ReturnsDecryptedSecret_WhenFound()
    {
        // Arrange
        var secret = new TextSecret { Text = "secret-value" };
        var serialized = JsonSerializer.Serialize(secret);

        // The encrypted data needs to be base64-encoded (what IDataProtector.Protect returns)
        // When Unprotect is called with a string, it base64-decodes it first
        var encryptedBytes = System.Text.Encoding.UTF8.GetBytes("raw-encrypted-data");
        var encrypted = Convert.ToBase64String(encryptedBytes);

        var document = new SecretsDocument
        {
            Secrets =
            {
                ["test-secret"] = new SecretEntry
                {
                    Name = "test-secret",
                    Type = typeof(TextSecret).FullName,
                    EncryptedData = encrypted,
                },
            },
        };

        _documentManagerMock
            .Setup(m => m.GetOrCreateImmutableAsync())
            .ReturnsAsync(document);

        // The Unprotect(string) extension:
        // 1. Base64-decodes the input to get bytes
        // 2. Calls Unprotect(byte[])
        // 3. UTF-8 decodes the result to string
        _dataProtectorMock
            .Setup(p => p.Unprotect(It.IsAny<byte[]>()))
            .Returns(System.Text.Encoding.UTF8.GetBytes(serialized));

        // Act
        var result = await _store.GetSecretAsync<TextSecret>("test-secret");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("secret-value", result.Text);
    }

    [Fact]
    public async Task GetSecretAsync_ReturnsNull_WhenDecryptionFails()
    {
        // Arrange
        var document = new SecretsDocument
        {
            Secrets =
            {
                ["test-secret"] = new SecretEntry
                {
                    Name = "test-secret",
                    Type = typeof(TextSecret).FullName,
                    EncryptedData = "corrupted-data",
                },
            },
        };

        _documentManagerMock
            .Setup(m => m.GetOrCreateImmutableAsync())
            .ReturnsAsync(document);

        _dataProtectorMock
            .Setup(p => p.Unprotect(It.IsAny<byte[]>()))
            .Throws(new Exception("Decryption failed"));

        // Act
        var result = await _store.GetSecretAsync<TextSecret>("test-secret");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SaveSecretAsync_EncryptsAndSavesSecret()
    {
        // Arrange
        var secret = new TextSecret { Text = "new-secret-value" };
        var serialized = JsonSerializer.Serialize(secret);
        var encrypted = "encrypted-new-data";

        var document = new SecretsDocument();

        _documentManagerMock
            .Setup(m => m.GetOrCreateMutableAsync())
            .ReturnsAsync(document);

        _dataProtectorMock
            .Setup(p => p.Protect(It.IsAny<byte[]>()))
            .Returns(System.Text.Encoding.UTF8.GetBytes(encrypted));

        // Act
        await _store.SaveSecretAsync("new-secret", secret);

        // Assert
        Assert.True(document.Secrets.ContainsKey("new-secret"));
        Assert.Equal("new-secret", document.Secrets["new-secret"].Name);
        Assert.Equal(typeof(TextSecret).FullName, document.Secrets["new-secret"].Type);

        _documentManagerMock.Verify(m => m.UpdateAsync(document, null), Times.Once);
    }

    [Fact]
    public async Task SaveSecretAsync_UpdatesExistingSecret_PreservingCreatedDate()
    {
        // Arrange
        var originalCreatedDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var existingEntry = new SecretEntry
        {
            Name = "existing-secret",
            Type = typeof(TextSecret).FullName,
            EncryptedData = "old-encrypted",
            CreatedUtc = originalCreatedDate,
            UpdatedUtc = originalCreatedDate,
        };

        var document = new SecretsDocument
        {
            Secrets = { ["existing-secret"] = existingEntry },
        };

        var newSecret = new TextSecret { Text = "updated-value" };

        _documentManagerMock
            .Setup(m => m.GetOrCreateMutableAsync())
            .ReturnsAsync(document);

        _dataProtectorMock
            .Setup(p => p.Protect(It.IsAny<byte[]>()))
            .Returns(System.Text.Encoding.UTF8.GetBytes("new-encrypted"));

        // Act
        await _store.SaveSecretAsync("existing-secret", newSecret);

        // Assert
        Assert.Equal(originalCreatedDate, document.Secrets["existing-secret"].CreatedUtc);
        Assert.True(document.Secrets["existing-secret"].UpdatedUtc > originalCreatedDate);
    }

    [Fact]
    public async Task RemoveSecretAsync_RemovesSecret_WhenExists()
    {
        // Arrange
        var document = new SecretsDocument
        {
            Secrets =
            {
                ["to-remove"] = new SecretEntry { Name = "to-remove" },
            },
        };

        _documentManagerMock
            .Setup(m => m.GetOrCreateMutableAsync())
            .ReturnsAsync(document);

        // Act
        await _store.RemoveSecretAsync("to-remove");

        // Assert
        Assert.False(document.Secrets.ContainsKey("to-remove"));
        _documentManagerMock.Verify(m => m.UpdateAsync(document, null), Times.Once);
    }

    [Fact]
    public async Task RemoveSecretAsync_DoesNothing_WhenSecretNotFound()
    {
        // Arrange
        var document = new SecretsDocument();

        _documentManagerMock
            .Setup(m => m.GetOrCreateMutableAsync())
            .ReturnsAsync(document);

        // Act
        await _store.RemoveSecretAsync("non-existent");

        // Assert
        _documentManagerMock.Verify(m => m.UpdateAsync(It.IsAny<SecretsDocument>(), null), Times.Never);
    }

    [Fact]
    public async Task GetSecretInfosAsync_ReturnsInfoForAllSecrets()
    {
        // Arrange
        var document = new SecretsDocument
        {
            Secrets =
            {
                ["secret1"] = new SecretEntry
                {
                    Name = "secret1",
                    Type = typeof(TextSecret).FullName,
                    CreatedUtc = DateTime.UtcNow.AddDays(-1),
                    UpdatedUtc = DateTime.UtcNow,
                },
                ["secret2"] = new SecretEntry
                {
                    Name = "secret2",
                    Type = typeof(RsaKeySecret).FullName,
                    CreatedUtc = DateTime.UtcNow.AddDays(-2),
                    UpdatedUtc = DateTime.UtcNow.AddDays(-1),
                },
            },
        };

        _documentManagerMock
            .Setup(m => m.GetOrCreateImmutableAsync())
            .ReturnsAsync(document);

        // Act
        var result = (await _store.GetSecretInfosAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);

        var info1 = result.First(i => i.Name == "secret1");
        Assert.Equal("Database", info1.Store);
        Assert.Equal(typeof(TextSecret).FullName, info1.Type);

        var info2 = result.First(i => i.Name == "secret2");
        Assert.Equal("Database", info2.Store);
        Assert.Equal(typeof(RsaKeySecret).FullName, info2.Type);
    }

    [Fact]
    public async Task GetSecretInfosAsync_ReturnsEmpty_WhenNoSecrets()
    {
        // Arrange
        var document = new SecretsDocument();

        _documentManagerMock
            .Setup(m => m.GetOrCreateImmutableAsync())
            .ReturnsAsync(document);

        // Act
        var result = await _store.GetSecretInfosAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task SaveSecretAsync_ThrowsArgumentNullException_WhenNameIsNull()
    {
        // Arrange
        var secret = new TextSecret { Text = "value" };

        // Act & Assert (ArgumentException.ThrowIfNullOrEmpty throws ArgumentNullException for null)
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _store.SaveSecretAsync<TextSecret>(null, secret));
    }

    [Fact]
    public async Task SaveSecretAsync_ThrowsArgumentNullException_WhenSecretIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _store.SaveSecretAsync<TextSecret>("name", null));
    }
}
