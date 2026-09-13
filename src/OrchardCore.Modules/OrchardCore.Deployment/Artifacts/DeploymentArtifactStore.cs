using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace OrchardCore.Deployment.Artifacts;

internal sealed class DeploymentArtifactStore
{
    private readonly string _root;
    private readonly DeploymentArtifactOptions _options;
    private readonly IClock _clock;

    public DeploymentArtifactStore(IOptions<ShellOptions> shells, ShellSettings tenant,
        IOptions<DeploymentArtifactOptions> options, IClock clock)
    {
        _root = Path.Combine(shells.Value.ShellsApplicationDataPath, shells.Value.ShellsContainerName, tenant.Name, "DeploymentArtifacts");
        _options = options.Value;
        _clock = clock;
    }

    public async Task<DeploymentArtifact> CreateAsync(string owner, DeploymentArtifactKind kind, string fileName, string contentType, Stream input, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(owner);
        ArgumentNullException.ThrowIfNull(input);
        if (!Enum.IsDefined(kind)) { throw new ArgumentOutOfRangeException(nameof(kind)); }
        if (string.IsNullOrWhiteSpace(fileName) || fileName.IndexOfAny(['/', '\\', ':']) >= 0 || fileName.Any(char.IsControl)
            || contentType is not ("application/zip" or "application/json"))
        {
            throw new ArgumentException("A package filename and supported content type are required.");
        }
        if (_options.MaxBytes <= 0 || _options.Lifetime <= TimeSpan.Zero) { throw new InvalidOperationException("Artifact limits must be positive."); }
        var id = Guid.NewGuid().ToString("n");
        var folder = Folder(id);
        if (OperatingSystem.IsWindows()) { Directory.CreateDirectory(folder); }
        else { Directory.CreateDirectory(folder, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute); }
        try
        {
            using (var guard = new FileStream(Path.Combine(folder, "lease"), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
            {
                long length = 0;
                using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
                await using (var output = new FileStream(Path.Combine(folder, "content"), FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    var buffer = new byte[81920];
                    int read;
                    while ((read = await input.ReadAsync(buffer, cancellationToken)) != 0)
                    {
                        if (read > _options.MaxBytes - length) { throw new InvalidDataException("Artifact size limit exceeded."); }
                        hash.AppendData(buffer, 0, read);
                        await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                        length += read;
                    }
                }
                var created = _clock.UtcNow;
                var artifact = new DeploymentArtifact
                {
                    Id = id, Owner = owner, Kind = kind, FileName = fileName, ContentType = contentType, Length = length,
                    Sha256 = Convert.ToHexStringLower(hash.GetHashAndReset()), CreatedUtc = created, ExpiresUtc = created + _options.Lifetime,
                };
                await File.WriteAllTextAsync(Path.Combine(folder, "pending.json"), JsonSerializer.Serialize(artifact), cancellationToken);
                File.Move(Path.Combine(folder, "pending.json"), Path.Combine(folder, "metadata.json"));
                return artifact;
            }
        }
        catch
        {
            Directory.Delete(folder, recursive: true);
            throw;
        }
    }

    public async Task<DeploymentArtifact> FindAsync(string id, string owner, bool includeExpired = false)
    {
        if (!ValidId(id) || string.IsNullOrEmpty(owner)) { return null; }
        var artifact = await ReadAsync(id);
        return artifact?.Owner == owner && (includeExpired || artifact.ExpiresUtc > _clock.UtcNow) ? artifact : null;
    }

    public async Task<DeploymentArtifactLease> OpenAsync(string id, string owner)
    {
        if (!ValidId(id) || string.IsNullOrEmpty(owner)) { return null; }
        FileStream guard;
        try { guard = new FileStream(Path.Combine(Folder(id), "lease"), FileMode.Open, FileAccess.Read, FileShare.Read); }
        catch (IOException) { return null; }
        try
        {
            var artifact = await FindAsync(id, owner);
            if (artifact is null) { guard.Dispose(); return null; }
            var stream = new FileStream(Path.Combine(Folder(id), "content"), FileMode.Open, FileAccess.Read, FileShare.Read);
            return new DeploymentArtifactLease(artifact, stream, guard);
        }
        catch
        {
            guard.Dispose();
            throw;
        }
    }

    public Task<ArtifactDeleteResult> DeleteAsync(string id, string owner) => DeleteAsync(id, owner, expiredOnly: false);

    public async Task<int> CleanupAsync(int take = 100, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(take);
        cancellationToken.ThrowIfCancellationRequested();
        if (!Directory.Exists(_root)) { return 0; }
        var deleted = 0;
        foreach (var folder in Directory.EnumerateDirectories(_root))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var id = Path.GetFileName(folder);
            if (!ValidId(id)) { continue; }
            if (await DeleteAsync(id, null, expiredOnly: true) == ArtifactDeleteResult.Deleted || await CleanupOrphanAsync(id))
            {
                deleted++;
                if (deleted == take) { break; }
            }
        }
        return deleted;
    }

    private async Task<bool> CleanupOrphanAsync(string id)
    {
        var folder = Folder(id);
        if (!Directory.Exists(folder) || Directory.GetLastWriteTimeUtc(folder) + _options.Lifetime > _clock.UtcNow
            || await ReadAsync(id) is not null)
        {
            return false;
        }
        FileStream guard;
        try { guard = new FileStream(Path.Combine(folder, "lease"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
        catch (IOException) { return false; }
        using (guard)
        {
            // An upload may have published its metadata while we were acquiring the guard.
            if (await ReadAsync(id) is not null) { return false; }
            foreach (var file in Directory.EnumerateFiles(folder).Where(file => Path.GetFileName(file) != "lease")) { File.Delete(file); }
        }
        try { Directory.Delete(folder, recursive: true); }
        catch (IOException) { return false; }
        return true;
    }

    private async Task<ArtifactDeleteResult> DeleteAsync(string id, string owner, bool expiredOnly)
    {
        if (!ValidId(id) || (!expiredOnly && string.IsNullOrEmpty(owner))) { return ArtifactDeleteResult.Missing; }
        var artifact = await ReadAsync(id);
        if (artifact is null || (!expiredOnly && artifact.Owner != owner) || (expiredOnly && artifact.ExpiresUtc > _clock.UtcNow))
        {
            return ArtifactDeleteResult.Missing;
        }
        var folder = Folder(id);
        FileStream guard;
        try { guard = new FileStream(Path.Combine(folder, "lease"), FileMode.Open, FileAccess.ReadWrite, FileShare.None); }
        catch (FileNotFoundException) { return ArtifactDeleteResult.Missing; }
        catch (DirectoryNotFoundException) { return ArtifactDeleteResult.Missing; }
        catch (IOException) { return ArtifactDeleteResult.Busy; }
        using (guard)
        {
            File.Delete(Path.Combine(folder, "metadata.json"));
            File.Delete(Path.Combine(folder, "content"));
        }
        try { Directory.Delete(folder, recursive: true); }
        catch (IOException) { /* An absent artifact can leave an empty lease directory during a read race. */ }
        return ArtifactDeleteResult.Deleted;
    }

    private async Task<DeploymentArtifact> ReadAsync(string id)
    {
        try { return JsonSerializer.Deserialize<DeploymentArtifact>(await File.ReadAllTextAsync(Path.Combine(Folder(id), "metadata.json"))); }
        catch (FileNotFoundException) { return null; }
        catch (DirectoryNotFoundException) { return null; }
    }

    private string Folder(string id) => Path.Combine(_root, id);
    private static bool ValidId(string id) => id is not null && Guid.TryParseExact(id, "N", out _);
}
