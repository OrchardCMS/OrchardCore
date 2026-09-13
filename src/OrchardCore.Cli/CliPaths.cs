using System.Security.Cryptography;
using System.Text;

namespace OrchardCore.Cli;

internal sealed class CliPaths
{
    public CliPaths(string rootDirectory, string? credentialsDirectory = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);

        RootDirectory = rootDirectory;
        ConfigFilePath = Path.Combine(rootDirectory, "contexts.json");
        CacheDirectory = Path.Combine(rootDirectory, "cache");
        CredentialsDirectory = credentialsDirectory ?? Path.Combine(rootDirectory, "credentials");
    }

    public string RootDirectory { get; }

    public string ConfigFilePath { get; }

    public string CacheDirectory { get; }

    public string CredentialsDirectory { get; }

    public void EnsureRootDirectory()
    {
        Directory.CreateDirectory(RootDirectory);
        SetOwnerOnlyDirectory(RootDirectory);
    }

    public static CliPaths CreateDefault()
    {
        var configuredRoot = global::System.Environment.GetEnvironmentVariable("OC_CONFIG_HOME");
        if (!string.IsNullOrWhiteSpace(configuredRoot))
        {
            if (!Path.IsPathFullyQualified(configuredRoot))
            {
                throw new CliException("OC_CONFIG_HOME must be an absolute directory path.");
            }

            return new CliPaths(configuredRoot);
        }

        return new CliPaths(GetDefaultRootDirectory(), OperatingSystem.IsWindows()
            ? null : Path.Combine(GetUserHomeDirectory(), ".orchardcore", "credentials"));
    }

    public static string NormalizeTenantUrl(string tenantUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantUrl);

        var builder = new UriBuilder(CliUriPolicy.RequireSecureEndpoint(tenantUrl))
        {
            Query = string.Empty,
            Fragment = string.Empty,
        };

        if (!builder.Path.EndsWith('/'))
        {
            builder.Path = $"{builder.Path.TrimEnd('/')}/";
        }

        return builder.Uri.AbsoluteUri;
    }

    public string GetCacheFilePath(string tenantUrl, CacheKind kind)
    {
        var directory = Path.Combine(CacheDirectory, ComputeContextKey(tenantUrl));
        Directory.CreateDirectory(directory);
        SetOwnerOnlyDirectory(CacheDirectory);
        SetOwnerOnlyDirectory(directory);

        return Path.Combine(directory, kind switch
        {
            CacheKind.Manifest => "manifest.json",
            CacheKind.OpenApi => "openapi.json",
            CacheKind.Documentation => "documentation.json",
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        });
    }

    public string GetCredentialFilePath(string contextName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contextName);

        EnsureRootDirectory();
        var credentialsRoot = Path.GetDirectoryName(CredentialsDirectory)
            ?? throw new InvalidOperationException("The credentials directory must have a parent directory.");
        Directory.CreateDirectory(credentialsRoot);
        SetOwnerOnlyDirectory(credentialsRoot);
        Directory.CreateDirectory(CredentialsDirectory);
        SetOwnerOnlyDirectory(CredentialsDirectory);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(contextName.Trim().ToUpperInvariant()));
        return Path.Combine(CredentialsDirectory, $"{Convert.ToHexString(bytes).ToLowerInvariant()}.json");
    }

    private static string ComputeContextKey(string tenantUrl)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(NormalizeTenantUrl(tenantUrl)));
        return Convert.ToHexString(bytes).ToLowerInvariant()[..16];
    }

    public static void SetOwnerOnlyFile(string path)
    {
        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }

    internal static void SetOwnerOnlyDirectory(string path)
    {
        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(
                path,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }
    }

    // Retain the original storage location so Pomi reuses existing contexts and caches.
    private static string GetDefaultRootDirectory()
    {
        string baseDirectory;

        if (OperatingSystem.IsWindows())
        {
            baseDirectory = global::System.Environment.GetFolderPath(
                global::System.Environment.SpecialFolder.ApplicationData,
                global::System.Environment.SpecialFolderOption.Create);
            return Path.Combine(baseDirectory, "OrchardCore", "oc");
        }

        if (OperatingSystem.IsMacOS())
        {
            baseDirectory = Path.Combine(GetUserHomeDirectory(), "Library", "Application Support");
            return Path.Combine(baseDirectory, "OrchardCore", "oc");
        }

        baseDirectory = global::System.Environment.GetEnvironmentVariable("XDG_CONFIG_HOME")
            ?? Path.Combine(GetUserHomeDirectory(), ".config");
        return Path.Combine(baseDirectory, "orchardcore", "oc");
    }

    private static string GetUserHomeDirectory()
    {
        var home = global::System.Environment.GetFolderPath(
            global::System.Environment.SpecialFolder.UserProfile,
            global::System.Environment.SpecialFolderOption.Create);
        if (string.IsNullOrWhiteSpace(home) || !Path.IsPathFullyQualified(home))
        {
            throw new InvalidOperationException("The user home directory could not be resolved.");
        }

        return home;
    }
}
