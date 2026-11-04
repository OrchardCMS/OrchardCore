using System.Text.Json;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Cli;

internal sealed class ContextStore
{
    private static readonly TimeSpan _lockTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan _lockRetryDelay = TimeSpan.FromMilliseconds(50);

    private readonly CliPaths _paths;
    private ConfigurationSnapshot? _baseline;

    public ContextStore(CliPaths paths)
    {
        _paths = paths;
    }

    public async Task<CliConfiguration> LoadAsync(CancellationToken cancellationToken)
    {
        _paths.EnsureRootDirectory();

        var configuration = await ReadAsync(cancellationToken);
        _baseline = ConfigurationSnapshot.Create(configuration);
        return configuration;
    }

    public Task SaveAsync(CliConfiguration configuration, CancellationToken cancellationToken) =>
        UpdateAsync(configuration, static _ => Task.CompletedTask, cancellationToken);

    /// <summary>
    /// Runs an update while holding the configuration lock, so that it sees and keeps what other
    /// processes saved since this one loaded the configuration. <paramref name="configuration"/>
    /// receives the merged state.
    /// </summary>
    public async Task UpdateAsync(CliConfiguration configuration, Func<CliConfiguration, Task> update, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(update);
        _paths.EnsureRootDirectory();

        await using var configurationLock = await AcquireLockAsync(cancellationToken);
        Merge(configuration, await ReadAsync(cancellationToken), _baseline);
        await update(configuration);
        await WriteAsync(configuration, cancellationToken);
        _baseline = ConfigurationSnapshot.Create(configuration);
    }

    private async Task<CliConfiguration> ReadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_paths.ConfigFilePath))
        {
            return new CliConfiguration();
        }

        await using var stream = AtomicFile.OpenRead(_paths.ConfigFilePath);
        return await JsonSerializer.DeserializeAsync(stream, CliJsonContext.Default.CliConfiguration, cancellationToken)
            ?? new CliConfiguration();
    }

    private async Task WriteAsync(CliConfiguration configuration, CancellationToken cancellationToken)
    {
        var temporaryPath = $"{_paths.ConfigFilePath}.{Guid.NewGuid():n}.tmp";
        try
        {
            await using (var stream = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(stream, configuration, CliJsonContext.Default.CliConfiguration, cancellationToken);
            }

            CliPaths.SetOwnerOnlyFile(temporaryPath);
            await AtomicFile.ReplaceAsync(temporaryPath, _paths.ConfigFilePath, cancellationToken);
        }
        finally
        {
            File.Delete(temporaryPath);
        }
    }

    // FileShare.None is an exclusive lock across processes: a sharing mode on Windows, flock elsewhere.
    // The file stays in place, as deleting it could let two processes lock different files.
    private async Task<FileStream> AcquireLockAsync(CancellationToken cancellationToken)
    {
        var path = _paths.ConfigFilePath + ".lock";
        var deadline = DateTimeOffset.UtcNow + _lockTimeout;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                CliPaths.SetOwnerOnlyFile(path);
                return stream;
            }
            catch (IOException) when (DateTimeOffset.UtcNow < deadline)
            {
                await Task.Delay(_lockRetryDelay, cancellationToken);
            }
            catch (IOException exception)
            {
                throw new CliException($"Another pomi process kept '{path}' locked for {_lockTimeout.TotalSeconds:0} seconds, so the contexts were not saved: {exception.Message}");
            }
        }
    }

    // A three-way merge against what this process loaded: its own changes win, and everything it
    // didn't touch comes from the file, so a run never undoes or drops what another run saved.
    private static void Merge(CliConfiguration configuration, CliConfiguration saved, ConfigurationSnapshot? baseline)
    {
        var merged = new List<TenantContextRecord>();
        foreach (var context in configuration.Contexts)
        {
            if (baseline?.Contexts.TryGetValue(context.Name, out var original) is not true)
            {
                // Added by this process.
                merged.Add(context);
            }
            else if (FindByName(saved, context.Name) is { } savedContext)
            {
                merged.Add(ConfigurationSnapshot.Serialize(context) == original ? savedContext : context);
            }

            // Otherwise another process deleted it after this one loaded it.
        }

        foreach (var savedContext in saved.Contexts)
        {
            if (baseline?.Contexts.ContainsKey(savedContext.Name) is not true && FindByName(configuration, savedContext.Name) is null)
            {
                // Added by another process. A context this process loaded and no longer has is one it deleted.
                merged.Add(savedContext);
            }
        }

        var currentContextChanged = !string.Equals(configuration.CurrentContext, baseline?.CurrentContext, StringComparison.OrdinalIgnoreCase);
        var currentContext = currentContextChanged ? configuration.CurrentContext : saved.CurrentContext;

        configuration.Contexts.Clear();
        configuration.Contexts.AddRange(merged);
        configuration.CurrentContext = currentContext is not null && FindByName(configuration, currentContext) is null
            ? merged.FirstOrDefault()?.Name
            : currentContext;
    }

    private static TenantContextRecord? FindByName(CliConfiguration configuration, string name) =>
        configuration.Contexts.FirstOrDefault(context => string.Equals(context.Name, name, StringComparison.OrdinalIgnoreCase));

    public static TenantContextRecord? FindContext(CliConfiguration configuration, string? name)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var resolvedName = string.IsNullOrWhiteSpace(name) ? configuration.CurrentContext : name;

        return resolvedName is null
            ? null
            : configuration.Contexts.FirstOrDefault(context => string.Equals(context.Name, resolvedName, StringComparison.OrdinalIgnoreCase));
    }

    public static TenantContextRecord AddOrUpdate(CliConfiguration configuration, string name, RemoteManagementManifest manifest, bool makeCurrent)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(manifest);

        var managementManifestUrl = manifest.ManagementManifestUrl ?? throw new InvalidOperationException("The management manifest URL is required.");
        var tenantBuilder = new UriBuilder(managementManifestUrl);
        var pathValue = tenantBuilder.Path;
        const string manifestPathSuffix = "/api/management/manifest";
        if (pathValue.EndsWith(manifestPathSuffix, StringComparison.OrdinalIgnoreCase))
        {
            pathValue = pathValue[..^manifestPathSuffix.Length];
        }

        tenantBuilder.Path = string.IsNullOrEmpty(pathValue) ? "/" : pathValue;
        var normalizedUrl = CliPaths.NormalizeTenantUrl(tenantBuilder.Uri.AbsoluteUri);

        var existing = configuration.Contexts.FirstOrDefault(context => string.Equals(context.Name, name, StringComparison.OrdinalIgnoreCase));
        existing ??= new TenantContextRecord
        {
            Name = name,
            AddedAt = DateTimeOffset.UtcNow,
        };

        existing.Name = name;
        existing.TenantUrl = normalizedUrl;
        existing.TenantId = manifest.TenantId;
        existing.ProductVersion = manifest.ProductVersion;
        existing.Authority = manifest.Authentication.Authority?.AbsoluteUri;
        existing.ClientId = manifest.Authentication.ClientId;
        existing.ManagementManifestUrl = manifest.ManagementManifestUrl?.AbsoluteUri;
        existing.OpenApiUrl = manifest.OpenApiUrl?.AbsoluteUri;
        existing.DocumentationIndexUrl = manifest.DocumentationIndexUrl?.AbsoluteUri;
        existing.GrantTypes = [.. manifest.Authentication.GrantTypes];
        existing.Scopes = [.. manifest.Authentication.Scopes];

        if (!configuration.Contexts.Contains(existing))
        {
            configuration.Contexts.Add(existing);
        }

        if (makeCurrent || string.IsNullOrWhiteSpace(configuration.CurrentContext))
        {
            configuration.CurrentContext = existing.Name;
        }

        return existing;
    }

    public static bool Delete(CliConfiguration configuration, string name)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var existing = configuration.Contexts.FirstOrDefault(context => string.Equals(context.Name, name, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            return false;
        }

        configuration.Contexts.Remove(existing);
        if (string.Equals(configuration.CurrentContext, existing.Name, StringComparison.OrdinalIgnoreCase))
        {
            configuration.CurrentContext = configuration.Contexts.FirstOrDefault()?.Name;
        }

        return true;
    }
}

internal sealed class ConfigurationSnapshot
{
    private ConfigurationSnapshot(string? currentContext, Dictionary<string, string> contexts)
    {
        CurrentContext = currentContext;
        Contexts = contexts;
    }

    public string? CurrentContext { get; }

    public Dictionary<string, string> Contexts { get; }

    public static ConfigurationSnapshot Create(CliConfiguration configuration)
    {
        var contexts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var context in configuration.Contexts)
        {
            contexts[context.Name] = Serialize(context);
        }

        return new ConfigurationSnapshot(configuration.CurrentContext, contexts);
    }

    public static string Serialize(TenantContextRecord context) =>
        JsonSerializer.Serialize(context, CliJsonContext.Default.TenantContextRecord);
}
