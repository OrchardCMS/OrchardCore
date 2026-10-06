using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;

namespace OrchardCore.Secrets.Services;

public sealed class SecretStoreOperations : ISecretStoreOperations
{
    internal const string MutationLockKey = "OrchardCore.Secrets.Mutation";
    internal static readonly TimeSpan LockExpiration = TimeSpan.FromMinutes(15);

    private readonly ISecretManager _manager;
    private readonly IEnumerable<ISecretTypeProvider> _providers;
    private readonly IDistributedLock _distributedLock;
    private readonly ILogger _logger;

    public SecretStoreOperations(
        ISecretManager manager,
        IEnumerable<ISecretTypeProvider> providers,
        IDistributedLock distributedLock,
        ILogger<SecretStoreOperations> logger)
    {
        _manager = manager;
        _providers = providers;
        _distributedLock = distributedLock;
        _logger = logger;
    }

    public async Task<SecretStoreOperationResult> MoveSecretAsync(string name, string sourceStore, string destinationStore)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var (source, destination) = GetMoveStores(sourceStore, destinationStore);
        await using var locker = await AcquireMutationLockAsync(_distributedLock);
        return await MoveAsync(name, source, destination, Stopwatch.StartNew());
    }

    public async Task<IReadOnlyList<SecretStoreOperationResult>> MoveAllSecretsAsync(string sourceStore, string destinationStore)
    {
        var (source, destination) = GetMoveStores(sourceStore, destinationStore);
        await using var locker = await AcquireMutationLockAsync(_distributedLock);
        var elapsed = Stopwatch.StartNew();
        var names = (await source.GetSecretInfosAsync()).Select(info => info.Name).ToArray();
        var results = new List<SecretStoreOperationResult>(names.Length);
        foreach (var name in names)
        {
            results.Add(await MoveAsync(name, source, destination, elapsed));
        }

        return results;
    }

    private async Task<SecretStoreOperationResult> MoveAsync(string name, ISecretStore source, ISecretStore destination, Stopwatch elapsed)
    {
        var destinationSaved = false;
        var removingSource = false;
        try
        {
            if (IsTimedOut(elapsed))
            {
                return Failure(name, SecretStoreOperationFailure.OperationTimedOut);
            }

            var info = (await source.GetSecretInfosAsync()).SingleOrDefault(i => i.Name.Equals(name, StringComparison.Ordinal));
            if (info == null)
            {
                return Failure(name, SecretStoreOperationFailure.SourceMissing);
            }

            if ((await destination.GetSecretInfosAsync()).Any(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                return Failure(name, SecretStoreOperationFailure.DestinationCollision);
            }

            var secret = await source.GetSecretAsync<ISecret>(name);
            if (secret == null)
            {
                return Failure(name, SecretStoreOperationFailure.SourceMissing);
            }

            var provider = _providers.SingleOrDefault(p => p.SecretType == secret.GetType());
            if (provider == null)
            {
                return Failure(name, SecretStoreOperationFailure.UnsupportedType);
            }

            var serialized = provider.Serialize(secret);
            await destination.SaveSecretAsync(name, secret, new SecretSaveOptions
            {
                Description = info.Description,
                ExpiresUtc = info.ExpiresUtc,
            });
            destinationSaved = true;

            var saved = await destination.GetSecretAsync<ISecret>(name);
            var savedInfo = (await destination.GetSecretInfosAsync()).SingleOrDefault(i => i.Name.Equals(name, StringComparison.Ordinal));
            if (!SameValue(provider, serialized, saved) || savedInfo == null ||
                savedInfo.Description != info.Description || savedInfo.ExpiresUtc != info.ExpiresUtc)
            {
                return Failure(name, SecretStoreOperationFailure.VerificationFailed, destinationSaved);
            }

            // Store clients outside the manager do not participate in its lock.
            var current = await source.GetSecretAsync<ISecret>(name);
            var currentInfo = (await source.GetSecretInfosAsync()).SingleOrDefault(i => i.Name.Equals(name, StringComparison.Ordinal));
            if (!SameValue(provider, serialized, current) || currentInfo == null ||
                currentInfo.UpdatedUtc != info.UpdatedUtc || currentInfo.Description != info.Description ||
                currentInfo.ExpiresUtc != info.ExpiresUtc)
            {
                return Failure(name, SecretStoreOperationFailure.SourceChanged, destinationSaved);
            }

            if (IsTimedOut(elapsed))
            {
                return Failure(name, SecretStoreOperationFailure.OperationTimedOut, destinationSaved);
            }

            removingSource = true;
            await source.RemoveSecretAsync(name);
            if ((await source.GetSecretInfosAsync()).Any(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                return Failure(name, SecretStoreOperationFailure.SourceRemovalFailed, destinationSaved);
            }

            return new SecretStoreOperationResult { Name = name, Succeeded = true, DestinationSaved = true };
        }
        catch (Exception exception)
        {
            // Storage exceptions can contain response bodies with credentials.
            _logger.LogError("Secret move failed for '{SecretName}' from '{SourceStore}' to '{DestinationStore}' ({ExceptionType}). Destination saved: {DestinationSaved}.",
                name, source.Name, destination.Name, exception.GetType().Name, destinationSaved);
            return Failure(name, removingSource ? SecretStoreOperationFailure.SourceRemovalFailed : SecretStoreOperationFailure.StorageFailure, destinationSaved);
        }
    }

    private (ISecretStore Source, ISecretStore Destination) GetMoveStores(string sourceStore, string destinationStore)
    {
        var source = GetWritableStore(sourceStore);
        var destination = GetWritableStore(destinationStore);
        if (source == destination)
        {
            throw new InvalidOperationException("Select different source and destination stores.");
        }

        return (source, destination);
    }

    private ISecretStore GetWritableStore(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var store = _manager.GetStores().SingleOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("The selected secret store is not available.");
        if (store.IsReadOnly)
        {
            throw new InvalidOperationException("The selected secret store is read-only.");
        }

        return store;
    }

    private SecretStoreOperationResult Failure(string name, SecretStoreOperationFailure failure, bool destinationSaved = false)
    {
        _logger.LogWarning("Secret store operation failed for '{SecretName}': {Failure}. Destination saved: {DestinationSaved}.",
            name, failure, destinationSaved);
        return new SecretStoreOperationResult { Name = name, Failure = failure, DestinationSaved = destinationSaved };
    }

    private static bool SameValue(ISecretTypeProvider provider, string serialized, ISecret secret) =>
        secret != null && secret.GetType() == provider.SecretType && provider.Serialize(secret) == serialized;

    private static bool IsTimedOut(Stopwatch elapsed) => elapsed.Elapsed >= LockExpiration - TimeSpan.FromMinutes(1);

    internal static async Task<ILocker> AcquireMutationLockAsync(IDistributedLock distributedLock)
    {
        var (locker, locked) = await distributedLock.TryAcquireLockAsync(MutationLockKey, TimeSpan.FromSeconds(3), LockExpiration);
        if (!locked)
        {
            if (locker != null)
            {
                await locker.DisposeAsync();
            }

            throw new InvalidOperationException("Another secret store operation is in progress. Try again after it completes.");
        }

        return locker;
    }
}
