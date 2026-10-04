using System.Text.Json;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Providers;
using OrchardCore.Secrets.Services;
using ISecret = OrchardCore.Secrets.ISecret;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets;

public class SecretStoreOperationsTests
{
    private readonly MemoryStore _source = new("Database");
    private readonly MemoryStore _destination = new("AzureKeyVault");
    private readonly MemoryStore _other = new("Other");
    private readonly Mock<IDistributedLock> _lock = new();
    private readonly Mock<ILogger<SecretStoreOperations>> _logger = new();
    private readonly SecretManager _manager;
    private readonly SecretStoreOperations _operations;

    public SecretStoreOperationsTests()
    {
        _lock.Setup(l => l.TryAcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync((Mock.Of<ILocker>(), true));
        _manager = new SecretManager([_source, _destination, _other], NullLogger<SecretManager>.Instance, _lock.Object);
        _operations = CreateOperations(_manager, _lock.Object);
    }

    private SecretStoreOperations CreateOperations(ISecretManager manager, IDistributedLock locking) =>
        new(manager,
            [new TextSecretTypeProvider(Mock.Of<IStringLocalizer<TextSecretTypeProvider>>()),
             new RsaKeySecretTypeProvider(Mock.Of<IStringLocalizer<RsaKeySecretTypeProvider>>()),
             new X509SecretTypeProvider(Mock.Of<IStringLocalizer<X509SecretTypeProvider>>())],
            locking, _logger.Object);

    [Theory]
    [InlineData("Text")]
    [InlineData("RSA")]
    [InlineData("X509")]
    public async Task Move_PreservesValueTypeAndMetadataAndDeletesOnlySource(string type)
    {
        ISecret secret = type switch
        {
            "RSA" => new RsaKeySecret { PublicKey = "public", PrivateKey = "private" },
            "X509" => new X509Secret { Thumbprint = "thumbprint" },
            _ => new TextSecret { Text = "sensitive-value" },
        };
        var expires = DateTime.UtcNow.AddDays(10);
        _source.Add("secret", secret, "description", expires);
        _other.Add("secret", new TextSecret { Text = "other-value" });

        var result = await _operations.MoveSecretAsync("secret", "database", "azurekeyvault");

        Assert.True(result.Succeeded);
        Assert.True(result.DestinationSaved);
        Assert.Empty(_source.Entries);
        var saved = await _manager.GetSecretAsync<ISecret>("secret", "AzureKeyVault");
        Assert.Equal(secret.GetType(), saved.GetType());
        Assert.Equal(JsonSerializer.Serialize(secret, secret.GetType()), JsonSerializer.Serialize(saved, saved.GetType()));
        var info = Assert.Single(await _destination.GetSecretInfosAsync());
        Assert.Equal("description", info.Description);
        Assert.Equal(expires, info.ExpiresUtc);
        Assert.Equal("other-value", (await _other.GetSecretAsync<TextSecret>("secret")).Text);
        Assert.Equal(JsonSerializer.Serialize(saved, saved.GetType()), JsonSerializer.Serialize(await _manager.GetSecretAsync<ISecret>("secret"), saved.GetType()));
        Assert.Equal(1, _source.CommitCount);
        Assert.Equal(1, _destination.CommitCount);
    }

    [Fact]
    public async Task BulkMove_ContinuesAfterCollisionWithoutOverwritingIt()
    {
        _source.Add("first", new TextSecret { Text = "first-value" });
        _source.Add("collision", new TextSecret { Text = "source-value" });
        _source.Add("last", new TextSecret { Text = "last-value" });
        _destination.Add("COLLISION", new RsaKeySecret { PublicKey = "existing" });

        var results = await _operations.MoveAllSecretsAsync("Database", "AzureKeyVault");

        Assert.Equal(3, results.Count);
        Assert.Equal(2, results.Count(result => result.Succeeded));
        Assert.Equal(SecretStoreOperationFailure.DestinationCollision, Assert.Single(results, result => !result.Succeeded).Failure);
        Assert.Equal("collision", Assert.Single(_source.Entries).Key);
        Assert.Equal("existing", (await _destination.GetSecretAsync<RsaKeySecret>("COLLISION")).PublicKey);
        Assert.Equal("first-value", (await _destination.GetSecretAsync<TextSecret>("first")).Text);
        Assert.Equal("last-value", (await _destination.GetSecretAsync<TextSecret>("last")).Text);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task VerificationFailure_NeverRemovesSource(bool corruptValue)
    {
        _source.Add("secret", new TextSecret { Text = "original" }, "description");
        _destination.CorruptValue = corruptValue;
        _destination.CorruptMetadata = !corruptValue;

        var result = await _operations.MoveSecretAsync("secret", "Database", "AzureKeyVault");

        Assert.Equal(SecretStoreOperationFailure.VerificationFailed, result.Failure);
        Assert.True(result.DestinationSaved);
        Assert.Single(_source.Entries);
        Assert.Equal(0, _source.RemoveCount);
    }

    [Fact]
    public async Task FailedDestinationCommit_NeverRemovesSource()
    {
        _source.Add("secret", new TextSecret { Text = "original" });
        _destination.CommitFailure = new InvalidOperationException("storage failure containing sensitive-value");

        var result = await _operations.MoveSecretAsync("secret", "Database", "AzureKeyVault");

        Assert.False(result.Succeeded);
        Assert.False(result.DestinationSaved);
        Assert.Single(_source.Entries);
        Assert.Empty(_destination.Entries);
        Assert.Equal(0, _source.RemoveCount);
        Assert.All(_logger.Invocations.Where(call => call.Method.Name == nameof(ILogger.Log)), call =>
        {
            Assert.Null(call.Arguments[3]);
            Assert.DoesNotContain("sensitive-value", call.Arguments[2].ToString());
        });
        Assert.DoesNotContain("sensitive-value", JsonSerializer.Serialize(result));
    }

    [Fact]
    public async Task SourceChangeDuringCopy_IsDetectedBeforeDeletion()
    {
        _source.Add("secret", new TextSecret { Text = "original" });
        _destination.OnSave = () => _source.Add("secret", new TextSecret { Text = "rotated" });

        var result = await _operations.MoveSecretAsync("secret", "Database", "AzureKeyVault");

        Assert.Equal(SecretStoreOperationFailure.SourceChanged, result.Failure);
        Assert.True(result.DestinationSaved);
        Assert.Equal("rotated", (await _source.GetSecretAsync<TextSecret>("secret")).Text);
        Assert.Equal(0, _source.RemoveCount);
    }

    [Fact]
    public async Task FailedSourceDelete_ReportsPartialTransferAndRetainsDestination()
    {
        _source.Add("secret", new TextSecret { Text = "original" });
        _source.RemoveFailure = new InvalidOperationException("denied");

        var result = await _operations.MoveSecretAsync("secret", "Database", "AzureKeyVault");

        Assert.Equal(SecretStoreOperationFailure.SourceRemovalFailed, result.Failure);
        Assert.True(result.DestinationSaved);
        Assert.Single(_source.Entries);
        Assert.Single(_destination.Entries);
    }

    [Fact]
    public async Task FailedSourceCommit_IsNotReportedAsSuccessfulMove()
    {
        _source.Add("secret", new TextSecret { Text = "original" });
        _source.CommitFailure = new InvalidOperationException("commit failed");

        var result = await _operations.MoveSecretAsync("secret", "Database", "AzureKeyVault");

        Assert.Equal(SecretStoreOperationFailure.SourceRemovalFailed, result.Failure);
        Assert.True(result.DestinationSaved);
        Assert.Single(_source.Entries);
        Assert.Single(_destination.Entries);
    }

    [Theory]
    [InlineData("Database", "Database")]
    [InlineData("Missing", "AzureKeyVault")]
    [InlineData("Database", "Missing")]
    public async Task InvalidStores_AreRejectedBeforeMutation(string source, string destination)
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => _operations.MoveAllSecretsAsync(source, destination));
        Assert.Equal(0, _source.RemoveCount);
        Assert.Equal(0, _destination.CommitCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ReadOnlyStore_CannotBeSourceOrDestination(bool source)
    {
        (source ? _source : _destination).IsReadOnly = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => _operations.MoveAllSecretsAsync("Database", "AzureKeyVault"));
    }

    [Fact]
    public async Task MissingSecret_DoesNotCreateDestination()
    {
        var result = await _operations.MoveSecretAsync("missing", "Database", "AzureKeyVault");
        Assert.Equal(SecretStoreOperationFailure.SourceMissing, result.Failure);
        Assert.Empty(_destination.Entries);
    }

    [Fact]
    public async Task ExistingDelete_IsProviderScoped()
    {
        _source.Add("secret", new TextSecret { Text = "source" });
        _other.Add("secret", new TextSecret { Text = "other" });

        await _manager.RemoveSecretAsync("secret", "Database");
        Assert.Empty(_source.Entries);
        Assert.Single(_other.Entries);
    }

    [Fact]
    public async Task FailedLock_DoesNotMutateStores()
    {
        _lock.Setup(l => l.TryAcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync(((ILocker)null, false));
        _source.Add("secret", new TextSecret { Text = "original" });

        await Assert.ThrowsAsync<InvalidOperationException>(() => _operations.MoveAllSecretsAsync("Database", "AzureKeyVault"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => _manager.SaveSecretAsync("secret", new TextSecret { Text = "changed" }));
        await Assert.ThrowsAsync<InvalidOperationException>(() => _manager.RemoveSecretAsync("secret"));
        Assert.Single(_source.Entries);
        Assert.Empty(_destination.Entries);
    }

    [Fact]
    public async Task ManagerMutation_WaitsForInProgressMove()
    {
        using var locking = new LocalLock(NullLogger<LocalLock>.Instance);
        var manager = new SecretManager([_source, _destination], NullLogger<SecretManager>.Instance, locking);
        var operations = CreateOperations(manager, locking);
        _source.Add("secret", new TextSecret { Text = "original" });
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _destination.BeforeSave = async () =>
        {
            started.SetResult();
            await release.Task;
        };

        var move = operations.MoveSecretAsync("secret", "Database", "AzureKeyVault");
        await started.Task;
        var edit = manager.SaveSecretAsync("secret", new TextSecret { Text = "after-move" }, "AzureKeyVault");
        Assert.False(edit.IsCompleted);
        release.SetResult();

        Assert.True((await move).Succeeded);
        await edit;
        Assert.Empty(_source.Entries);
        Assert.Equal("after-move", (await _destination.GetSecretAsync<TextSecret>("secret")).Text);
    }

    internal sealed class MemoryStore : ISecretStore
    {
        public MemoryStore(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public bool IsReadOnly { get; set; }
        public Dictionary<string, (ISecret Secret, SecretInfo Info)> Entries { get; } = [];
        private readonly Dictionary<string, (ISecret Secret, SecretInfo Info)> _pending = [];
        private readonly HashSet<string> _removed = [];
        public bool CorruptValue { get; set; }
        public bool CorruptMetadata { get; set; }
        public Exception CommitFailure { get; set; }
        public Exception RemoveFailure { get; set; }
        public int CommitCount { get; private set; }
        public int RemoveCount { get; private set; }
        public Action OnSave { get; set; }
        public Func<Task> BeforeSave { get; set; }

        public void Add(string name, ISecret secret, string description = null, DateTime? expires = null) =>
            Entries[name] = (secret, new SecretInfo
            {
                Name = name,
                Store = Name,
                Type = secret.GetType().FullName,
                Description = description,
                ExpiresUtc = expires,
            });

        public Task<T> GetSecretAsync<T>(string name) where T : class, ISecret =>
            Task.FromResult(Entries.TryGetValue(name, out var entry) ? entry.Secret as T : null);

        public async Task SaveSecretAsync<T>(string name, T secret, SecretSaveOptions options = null) where T : class, ISecret
        {
            if (BeforeSave != null)
            {
                await BeforeSave();
                BeforeSave = null;
            }

            var copy = CorruptValue ? new TextSecret { Text = "wrong" }
                : (ISecret)JsonSerializer.Deserialize(JsonSerializer.Serialize(secret, secret.GetType()), secret.GetType());
            _pending[name] = (copy, new SecretInfo
            {
                Name = name,
                Store = Name,
                Type = copy.GetType().FullName,
                Description = CorruptMetadata ? "wrong" : options?.Description,
                ExpiresUtc = options?.ExpiresUtc,
            });
            OnSave?.Invoke();
            await CommitAsync();
        }

        public Task RemoveSecretAsync(string name)
        {
            RemoveCount++;
            if (RemoveFailure != null)
            {
                throw RemoveFailure;
            }

            _removed.Add(name);
            return CommitAsync();
        }

        private Task CommitAsync()
        {
            if (CommitFailure != null)
            {
                throw CommitFailure;
            }

            CommitCount++;
            foreach (var (name, entry) in _pending)
            {
                Entries[name] = entry;
            }

            foreach (var name in _removed)
            {
                Entries.Remove(name);
            }

            _pending.Clear();
            _removed.Clear();
            return Task.CompletedTask;
        }

        public Task<IEnumerable<SecretInfo>> GetSecretInfosAsync() =>
            Task.FromResult<IEnumerable<SecretInfo>>(Entries.Values.Select(entry => entry.Info).ToArray());

    }
}
