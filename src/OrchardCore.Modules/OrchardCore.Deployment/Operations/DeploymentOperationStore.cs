using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace OrchardCore.Deployment.Operations;

internal enum DeploymentOperationState { Pending, Running, Succeeded, Failed, Uncertain }
internal enum DeploymentOperationKind { Export, Import }

internal sealed record DeploymentOperation
{
    public string Id { get; init; }
    public string Owner { get; init; }
    public DeploymentOperationKind Kind { get; init; }
    public string Payload { get; init; }
    public DeploymentExecutionIdentity Identity { get; init; }
    public DeploymentOperationState State { get; init; }
    public string ArtifactId { get; init; }
    public string ErrorCode { get; init; }
    public DateTime CreatedUtc { get; init; }
    public DateTime UpdatedUtc { get; init; }
}

// State is committed independently of recipe transactions. A held filesystem guard
// identifies the sole live executor; abandoned Running records are never replayed.
internal sealed class DeploymentOperationStore
{
    private readonly string _root;
    private readonly IClock _clock;

    public DeploymentOperationStore(IOptions<ShellOptions> options, ShellSettings tenant, IClock clock)
    {
        _root = Path.Combine(options.Value.ShellsApplicationDataPath, options.Value.ShellsContainerName,
            tenant.Name, "DeploymentOperations");
        _clock = clock;
    }

    public async Task<DeploymentOperation> CreateAsync(string owner, string requestId, DeploymentOperationKind kind,
        string payload, CancellationToken cancellationToken, DeploymentExecutionIdentity identity = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(owner);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);
        ArgumentNullException.ThrowIfNull(payload);
        if (requestId.Length > 128 || !Enum.IsDefined(kind)) { throw new ArgumentException("Invalid deployment request."); }
        var id = RequestIdentity(owner, requestId);
        var folder = Folder(id);
        if (OperatingSystem.IsWindows()) { Directory.CreateDirectory(folder); }
        else { Directory.CreateDirectory(folder, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute); }
        var existing = await ReadAsync(id, cancellationToken);
        if (existing is not null) { return Match(existing, owner, kind, payload); }
        FileStream reservation;
        try { reservation = Guard(folder); }
        catch (IOException exception) when (exception.IsFileSharingViolation()
            || !OperatingSystem.IsWindows() && exception.HResult is 11 or 35)
        {
            existing = await ReadAsync(id, cancellationToken);
            if (existing is not null) { return Match(existing, owner, kind, payload); }
            throw new DeploymentOperationBusyException();
        }
        using var guard = reservation;
        existing = await ReadAsync(id, cancellationToken);
        if (existing is not null)
        {
            return Match(existing, owner, kind, payload);
        }
        var operation = new DeploymentOperation { Id = id, Owner = owner, Kind = kind, Payload = payload,
            Identity = identity, State = DeploymentOperationState.Pending, CreatedUtc = _clock.UtcNow, UpdatedUtc = _clock.UtcNow, };
        await WriteAsync(operation, cancellationToken);
        return operation;
    }

    public Task<DeploymentOperation> FindRequestAsync(string owner, string requestId, CancellationToken cancellationToken) =>
        FindAsync(RequestIdentity(owner, requestId), owner, cancellationToken);

    private static string RequestIdentity(string owner, string requestId) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new[] { owner, requestId }))));

    private static DeploymentOperation Match(DeploymentOperation existing, string owner, DeploymentOperationKind kind, string payload)
    {
        if (existing.Owner != owner || existing.Kind != kind || existing.Payload != payload)
        {
            throw new DeploymentRequestConflictException();
        }
        return existing;
    }

    public async Task<DeploymentOperation> FindAsync(string id, string owner, CancellationToken cancellationToken)
    {
        if (!ValidId(id)) { return null; }
        var operation = await ReadAsync(id, cancellationToken);
        return operation?.Owner == owner ? operation : null;
    }

    public async Task<DeploymentOperationClaim> ClaimAsync(string id, CancellationToken cancellationToken)
    {
        if (!ValidId(id) || !Directory.Exists(Folder(id))) { return null; }
        FileStream guard;
        try { guard = Guard(Folder(id)); }
        catch (IOException) { return null; }
        try
        {
            var operation = await ReadAsync(id, cancellationToken);
            if (operation?.State == DeploymentOperationState.Running)
            {
                await WriteAsync(operation with { State = DeploymentOperationState.Uncertain,
                    ErrorCode = "execution_interrupted", UpdatedUtc = _clock.UtcNow }, cancellationToken);
                return null;
            }
            if (operation?.State != DeploymentOperationState.Pending) { return null; }
            operation = operation with { State = DeploymentOperationState.Running, UpdatedUtc = _clock.UtcNow, };
            await WriteAsync(operation, cancellationToken);
            var claim = new DeploymentOperationClaim(this, operation, guard);
            guard = null;
            return claim;
        }
        finally { guard?.Dispose(); }
    }

    public async Task<IReadOnlyList<string>> PendingAsync(CancellationToken cancellationToken)
    {
        var result = new List<string>();
        if (!Directory.Exists(_root)) { return result; }
        foreach (var folder in Directory.EnumerateDirectories(_root))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var id = Path.GetFileName(folder);
            if (!ValidId(id)) { continue; }
            var operation = await ReadAsync(id, cancellationToken);
            if (operation?.State is DeploymentOperationState.Pending or DeploymentOperationState.Running)
            {
                result.Add(id);
            }
        }
        return result;
    }

    private static bool ValidId(string id) => id is { Length: 64 } && id.All(char.IsAsciiHexDigit);
    private string Folder(string id) => Path.Combine(_root, id);
    private static FileStream Guard(string folder) => new(Path.Combine(folder, "lease"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

    private async Task<DeploymentOperation> ReadAsync(string id, CancellationToken cancellationToken)
    {
        try
        {
            // Readers must permit atomic replacement on Windows as well as Unix.
            await using var stream = new FileStream(Path.Combine(Folder(id), "operation.json"), FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 4096, FileOptions.Asynchronous);
            return await JsonSerializer.DeserializeAsync<DeploymentOperation>(stream, cancellationToken: cancellationToken);
        }
        catch (FileNotFoundException) { return null; }
        catch (DirectoryNotFoundException) { return null; }
    }

    private async Task WriteAsync(DeploymentOperation operation, CancellationToken cancellationToken)
    {
        var folder = Folder(operation.Id);
        var temporary = Path.Combine(folder, "pending.json");
        await File.WriteAllTextAsync(temporary, JsonSerializer.Serialize(operation), cancellationToken);
        var destination = Path.Combine(folder, "operation.json");
        if (File.Exists(destination))
        {
            // ReplaceFile permits existing readers with delete sharing on Windows;
            // MoveFileEx overwrite can fail while the destination is open.
            File.Replace(temporary, destination, destinationBackupFileName: null);
        }
        else
        {
            File.Move(temporary, destination);
        }
    }

    internal sealed class DeploymentOperationClaim : IDisposable
    {
        private readonly DeploymentOperationStore _store;
        private readonly FileStream _guard;
        private bool _disposed;
        private bool _completed;
        public DeploymentOperation Operation { get; }

        internal DeploymentOperationClaim(DeploymentOperationStore store, DeploymentOperation operation, FileStream guard)
        {
            _store = store; Operation = operation; _guard = guard;
        }

        public async Task CompleteAsync(DeploymentOperationState state, string artifactId, string errorCode, CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_completed || state is not (DeploymentOperationState.Succeeded or DeploymentOperationState.Failed or DeploymentOperationState.Uncertain))
            {
                throw new InvalidOperationException("Invalid deployment completion.");
            }
            await _store.WriteAsync(Operation with { State = state, ArtifactId = artifactId, ErrorCode = errorCode,
                UpdatedUtc = _store._clock.UtcNow }, cancellationToken);
            _completed = true;
        }

        public void Dispose() { _disposed = true; _guard.Dispose(); }
    }
}

internal sealed class DeploymentRequestConflictException : InvalidOperationException;

internal sealed class DeploymentOperationBusyException : InvalidOperationException;
