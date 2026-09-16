using System.Text.Json;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Cli;

internal sealed class ContextStore
{
    private readonly CliPaths _paths;

    public ContextStore(CliPaths paths)
    {
        _paths = paths;
    }

    public async Task<CliConfiguration> LoadAsync(CancellationToken cancellationToken)
    {
        _paths.EnsureRootDirectory();

        if (!File.Exists(_paths.ConfigFilePath))
        {
            return new CliConfiguration();
        }

        await using var stream = AtomicFile.OpenRead(_paths.ConfigFilePath);
        return await JsonSerializer.DeserializeAsync(stream, CliJsonContext.Default.CliConfiguration, cancellationToken)
            ?? new CliConfiguration();
    }

    public async Task SaveAsync(CliConfiguration configuration, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _paths.EnsureRootDirectory();

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
