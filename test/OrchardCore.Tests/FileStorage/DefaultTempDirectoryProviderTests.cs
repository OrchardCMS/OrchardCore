using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;
using OrchardCore.Environment.Shell;
using OrchardCore.FileStorage;

namespace OrchardCore.Tests.FileStorage;

public sealed class DefaultTempDirectoryProviderTests : IDisposable
{
    private readonly string _basePath = Path.Combine(
        Path.GetTempPath(),
        nameof(DefaultTempDirectoryProviderTests),
        Guid.NewGuid().ToString("N"));

    private DefaultTempDirectoryProvider CreateStore(string tenantName = "TestTenant", string tempPath = null)
        => new(
            Options.Create(new TempDirectoryOptions { Path = tempPath ?? _basePath }),
            new ShellSettings { Name = tenantName });

    [Fact]
    public void GetRootDirectory_ReturnsTenantScopedPath_AndCreatesIt()
    {
        var store = CreateStore("Alpha");

        var root = store.GetRootDirectory();

        Assert.Equal(Path.Combine(_basePath, "Alpha"), root);
        Assert.True(Directory.Exists(root));
    }

    [Fact]
    public void GetRootDirectory_OnUnix_CreatesWithOwnerOnlyPermissions()
    {
        if (!OperatingSystem.IsWindows())
        {
            var root = CreateStore("Alpha").GetRootDirectory();

            Assert.Equal(
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute,
                File.GetUnixFileMode(root));

            return;
        }

        Assert.Skip("Unix file permissions are not supported on Windows.");
    }

    [Fact]
    public void DifferentTenants_GetIsolatedRoots()
    {
        var alpha = CreateStore("Alpha").GetRootDirectory();
        var beta = CreateStore("Beta").GetRootDirectory();

        Assert.NotEqual(alpha, beta);
    }

    [Fact]
    public void EmptyTempPath_FallsBackToSystemTempPath()
    {
        var store = new DefaultTempDirectoryProvider(
            Options.Create(new TempDirectoryOptions { Path = null }),
            new ShellSettings { Name = "Gamma" });

        var root = store.GetRootDirectory();

        Assert.Equal(Path.Combine(Path.GetTempPath(), "Gamma"), root);

        // Cleanup this one since it is created outside _basePath.
        Directory.Delete(root, recursive: true);
    }

    [Fact]
    public void GetRootDirectory_InSystemTempPath_RestrictsExistingDirectoryToOwner()
    {
        if (!OperatingSystem.IsWindows())
        {
            AssertRestrictsExistingDirectoryToOwner();

            return;
        }

        Assert.Skip("Unix file permissions are not supported on Windows.");
    }

    [UnsupportedOSPlatform("windows")]
    private static void AssertRestrictsExistingDirectoryToOwner()
    {
        var tenantName = Guid.NewGuid().ToString("N");
        var expectedRoot = Path.Combine(Path.GetTempPath(), tenantName);

        try
        {
            Directory.CreateDirectory(expectedRoot);
            File.SetUnixFileMode(expectedRoot, (UnixFileMode)0b111_111_111);

            var root = new DefaultTempDirectoryProvider(
                Options.Create(new TempDirectoryOptions { Path = null }),
                new ShellSettings { Name = tenantName }).GetRootDirectory();

            Assert.Equal(expectedRoot, root);
            Assert.Equal(
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute,
                File.GetUnixFileMode(root));
        }
        finally
        {
            Directory.Delete(expectedRoot, recursive: true);
        }
    }

    [Fact]
    public void GetRootDirectory_InSystemTempPath_OnWindows_RestrictsExistingDirectoryAccess()
    {
        if (OperatingSystem.IsWindows())
        {
            AssertRestrictsExistingDirectoryAccess();

            return;
        }

        Assert.Skip("Access control lists are only supported on Windows.");
    }

    [SupportedOSPlatform("windows")]
    private static void AssertRestrictsExistingDirectoryAccess()
    {
        var tenantName = Guid.NewGuid().ToString("N");
        var expectedRoot = Path.Combine(Path.GetTempPath(), tenantName);
        var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, domainSid: null);

        try
        {
            var existing = Directory.CreateDirectory(expectedRoot);
            var security = existing.GetAccessControl();
            security.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.FullControl, AccessControlType.Allow));
            existing.SetAccessControl(security);

            var root = new DefaultTempDirectoryProvider(
                Options.Create(new TempDirectoryOptions { Path = null }),
                new ShellSettings { Name = tenantName }).GetRootDirectory();

            var rootSecurity = new DirectoryInfo(root).GetAccessControl();
            var rules = rootSecurity.GetAccessRules(includeExplicit: true, includeInherited: true, typeof(SecurityIdentifier))
                .Cast<FileSystemAccessRule>()
                .ToArray();

            Assert.Equal(expectedRoot, root);
            Assert.True(rootSecurity.AreAccessRulesProtected);
            Assert.DoesNotContain(rules, rule => rule.IdentityReference == everyone);
            Assert.Contains(rules, rule => rule.IdentityReference == WindowsIdentity.GetCurrent().User);
        }
        finally
        {
            Directory.Delete(expectedRoot, recursive: true);
        }
    }

    [Fact]
    public void GetRootDirectory_InSystemTempPath_WhenRootIsSymbolicLink_Throws()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("Unix file permissions are not supported on Windows.");
        }

        var tenantName = Guid.NewGuid().ToString("N");
        var linkPath = Path.Combine(Path.GetTempPath(), tenantName);
        var target = Directory.CreateDirectory(Path.Combine(_basePath, "Target")).FullName;

        try
        {
            Directory.CreateSymbolicLink(linkPath, target);

            var store = new DefaultTempDirectoryProvider(
                Options.Create(new TempDirectoryOptions { Path = null }),
                new ShellSettings { Name = tenantName });

            Assert.Throws<InvalidOperationException>(store.GetRootDirectory);
        }
        finally
        {
            File.Delete(linkPath);
        }
    }

    [Fact]
    public void GetTempFileName_ReturnsUniquePathsUnderRoot_WithoutCreatingFile()
    {
        var store = CreateStore();

        var first = store.GetTempFileName();
        var second = store.GetTempFileName();

        Assert.StartsWith(Path.Combine(_basePath, "TestTenant"), first);
        Assert.NotEqual(first, second);
        Assert.False(File.Exists(first));
        // The root is ensured to exist even though the file is not created.
        Assert.True(Directory.Exists(Path.GetDirectoryName(first)));
    }

    [Theory]
    [InlineData(".zip", ".zip")]
    [InlineData("zip", ".zip")]
    public void GetTempFileName_HonorsExtension(string extension, string expectedExtension)
    {
        var store = CreateStore();

        var path = store.GetTempFileName(extension);

        Assert.Equal(expectedExtension, Path.GetExtension(path));
    }

    [Fact]
    public void CreateTempSubdirectory_CreatesUniqueExistingDirectories()
    {
        var store = CreateStore();

        var first = store.CreateTempSubdirectory();
        var second = store.CreateTempSubdirectory();

        Assert.NotEqual(first, second);
        Assert.True(Directory.Exists(first));
        Assert.True(Directory.Exists(second));
        Assert.StartsWith(Path.Combine(_basePath, "TestTenant"), first);
    }

    [Fact]
    public void CreateTempSubdirectory_AppliesPrefixToDirectoryName()
    {
        var store = CreateStore();

        var path = store.CreateTempSubdirectory("upload-");

        Assert.True(Directory.Exists(path));
        Assert.Equal(Path.Combine(_basePath, "TestTenant"), Path.GetDirectoryName(path));
        Assert.StartsWith("upload-", Path.GetFileName(path));
    }

    public void Dispose()
    {
        if (Directory.Exists(_basePath))
        {
            Directory.Delete(_basePath, recursive: true);
        }
    }
}
