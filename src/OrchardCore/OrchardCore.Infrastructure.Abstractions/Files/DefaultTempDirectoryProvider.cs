using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.FileStorage;

/// <summary>
/// Default filesystem based <see cref="ITempDirectoryProvider"/> implementation. Temporary files are placed under a
/// tenant-scoped sub-directory of <see cref="TempDirectoryOptions.Path"/> (or the operating system
/// temporary directory when no path is configured).
/// </summary>
public sealed class DefaultTempDirectoryProvider : ITempDirectoryProvider
{
    private const UnixFileMode OwnerOnly = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;

    private readonly string _rootPath;
    private readonly bool _isSharedSystemTempPath;
    private bool _rootVerified;

    public DefaultTempDirectoryProvider(
        IOptions<TempDirectoryOptions> options,
        ShellSettings shellSettings)
    {
        var basePath = options.Value.Path;

        if (string.IsNullOrWhiteSpace(basePath))
        {
            basePath = Path.GetTempPath();
            _isSharedSystemTempPath = true;
        }

        // Scope temporary files per tenant so that tenants can neither collide with nor observe each other's files.
        _rootPath = Path.Combine(basePath, shellSettings.Name);
    }

    public string GetRootDirectory()
    {
        CreateRootDirectory();

        return _rootPath;
    }

    public string CreateTempSubdirectory(string prefix = null)
    {
        CreateRootDirectory();

        string path;

        do
        {
            path = Path.Combine(_rootPath, prefix + Path.GetRandomFileName());
        }
        while (Directory.Exists(path) || File.Exists(path));

        Directory.CreateDirectory(path);

        return path;
    }

    public string GetTempFileName(string extension = null)
    {
        CreateRootDirectory();

        var name = Path.GetRandomFileName();

        if (!string.IsNullOrEmpty(extension))
        {
            name = Path.ChangeExtension(name, extension);
        }

        return Path.Combine(_rootPath, name);
    }

    private void CreateRootDirectory()
    {
        if (OperatingSystem.IsWindows())
        {
            Directory.CreateDirectory(_rootPath);
        }
        else
        {
            Directory.CreateDirectory(_rootPath, OwnerOnly);
        }

        if (_isSharedSystemTempPath && !_rootVerified)
        {
            EnsureRootIsPrivate();
            _rootVerified = true;
        }
    }

    // The system temporary directory can be writable by other local users ('/tmp' on Unix, or 'C:\Windows\Temp'
    // when the user profile is not loaded on Windows), and the tenant root has a predictable name.
    // 'Directory.CreateDirectory' does nothing when the path already exists, so another user could create it first
    // and then read, replace or redirect the files written to it, such as the recipes of an imported deployment
    // package. Make sure the root is a real directory that only the current user can access.
    private void EnsureRootIsPrivate()
    {
        var directory = new DirectoryInfo(_rootPath);

        if (directory.LinkTarget is not null)
        {
            throw new InvalidOperationException(
                $"The temporary directory '{_rootPath}' is a symbolic link. Remove it, or configure a private " +
                "location with the 'OrchardCore:TempDirectory:Path' setting.");
        }

        if (OperatingSystem.IsWindows())
        {
            RestrictToCurrentUser(directory);

            return;
        }

        try
        {
            // Changing the mode of a directory requires owning it, so this fails when it was created by another
            // user, and removes any access a pre-existing directory may have granted to other users.
            File.SetUnixFileMode(_rootPath, OwnerOnly);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw CreateOwnedByAnotherUserException(ex);
        }
    }

    [SupportedOSPlatform("windows")]
    private void RestrictToCurrentUser(DirectoryInfo directory)
    {
        using var identity = WindowsIdentity.GetCurrent();

        var user = identity.User;
        var owner = directory.GetAccessControl(AccessControlSections.Owner).GetOwner(typeof(SecurityIdentifier));

        // The objects created by an elevated administrator are owned by the 'Administrators' group by default. A
        // standard user cannot make that group the owner of a directory.
        if (owner != user &&
            !(owner is SecurityIdentifier sid &&
              sid.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid) &&
              identity.Groups?.Contains(sid) == true))
        {
            throw CreateOwnedByAnotherUserException(innerException: null);
        }

        // Replace the inherited permissions of the shared temporary directory with the ones of a user profile
        // temporary directory: the current user, the system and the administrators.
        var security = new DirectorySecurity();
        security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);

        foreach (var identityReference in new IdentityReference[]
        {
            user,
            new SecurityIdentifier(WellKnownSidType.LocalSystemSid, domainSid: null),
            new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, domainSid: null),
        })
        {
            security.AddAccessRule(new FileSystemAccessRule(
                identityReference,
                FileSystemRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Allow));
        }

        directory.SetAccessControl(security);
    }

    private InvalidOperationException CreateOwnedByAnotherUserException(Exception innerException)
        => new(
            $"The temporary directory '{_rootPath}' is owned by another user. Remove it, or configure a private " +
            "location with the 'OrchardCore:TempDirectory:Path' setting.",
            innerException);
}
