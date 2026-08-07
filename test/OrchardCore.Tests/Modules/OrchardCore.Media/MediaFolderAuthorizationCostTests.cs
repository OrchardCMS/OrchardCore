using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.FileStorage;
using OrchardCore.Media;
using OrchardCore.Media.Services;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

/// <summary>
/// Measures the file-store I/O cost of media folder authorization, to quantify the per-folder
/// authorization overhead discussed on https://github.com/OrchardCMS/OrchardCore/pull/19660.
///
/// The question these tests answer: does authorizing a folder cost storage round-trips, and does that
/// change when the "Secure Media" feature (OrchardCore.Media.Security) is enabled?
/// </summary>
public class MediaFolderAuthorizationCostTests
{
    private const string Folder = "photos";

    [Fact]
    public async Task ManageMediaFolder_WithoutSecureMedia_PerformsNoFileStoreIo()
    {
        var (services, store) = BuildProvider(secureMediaEnabled: false);
        var authorizationService = services.GetRequiredService<IAuthorizationService>();

        store.ResetCounters();

        var authorized = await authorizationService.AuthorizeAsync(User(), MediaPermissions.ManageMediaFolder, (object)Folder);

        Assert.True(authorized);

        // Pure string classification plus one resource-less permission check: no storage round-trips.
        Assert.Equal(0, store.TotalCalls);
    }

    [Fact]
    public async Task ManageMediaFolder_WithSecureMedia_PerformsFileStoreIoPerCheck()
    {
        var (services, store) = BuildProvider(secureMediaEnabled: true);
        var authorizationService = services.GetRequiredService<IAuthorizationService>();

        store.ResetCounters();

        var authorized = await authorizationService.AuthorizeAsync(User(), MediaPermissions.ManageMediaFolder, (object)Folder);

        Assert.True(authorized);

        // ManageMediaFolder now also evaluates ViewMedia for the path, and ViewMediaFolderAuthorizationHandler
        // stats the directory. On a remote store (Azure Blob, S3) each of these is a network round-trip.
        Assert.True(store.TotalCalls > 0,
            "Expected Secure Media to add at least one file-store call per authorization check.");
        Assert.Equal(1, store.CallsFor(nameof(IFileStore.GetDirectoryInfoAsync)));
    }

    [Fact]
    public async Task ManageMediaFolder_WithSecureMedia_CostScalesLinearlyWithFolderCount()
    {
        var (services, store) = BuildProvider(secureMediaEnabled: true);
        var authorizationService = services.GetRequiredService<IAuthorizationService>();

        var perCheck = await MeasureAsync(store, () => authorizationService.AuthorizeAsync(User(), MediaPermissions.ManageMediaFolder, (object)Folder));

        // Authorizing a listing of F subfolders means F + 1 checks (the folder itself plus each child).
        const int F = 10;
        var forListing = await MeasureAsync(store, async () =>
        {
            for (var i = 0; i <= F; i++)
            {
                await authorizationService.AuthorizeAsync(User(), MediaPermissions.ManageMediaFolder, (object)Folder);
            }
        });

        // No memoization exists today: the cost is strictly linear in the number of folders authorized.
        Assert.Equal(perCheck * (F + 1), forListing);
    }

    [Fact]
    public async Task ManageMediaFolder_WithGlobalPermission_ShortCircuitsSecureMediaIo()
    {
        // A user holding the global ViewMedia permission is authorized before the folder handler runs,
        // so no directory stat happens. This is the "short-circuit on the global permission" mitigation.
        var (services, store) = BuildProvider(
            secureMediaEnabled: true,
            grantedPermissions: [MediaPermissions.ManageMedia.Name, MediaPermissions.ViewMedia.Name]);

        var authorizationService = services.GetRequiredService<IAuthorizationService>();

        store.ResetCounters();

        var authorized = await authorizationService.AuthorizeAsync(User(), MediaPermissions.ManageMediaFolder, (object)Folder);

        Assert.True(authorized);
        Assert.Equal(0, store.TotalCalls);
    }

    [Theory]
    [InlineData("photos")]
    [InlineData("photos/2026")]
    [InlineData("photos/2026/january")]
    public async Task ViewMedia_DecisionDependsOnlyOnTheFirstTierFolder(string path)
    {
        // ViewMediaFolderAuthorizationHandler derives the permission from the path up to the FIRST
        // separator, so every descendant of 'photos' resolves to the same ViewMediaContent_photos
        // permission. Listing a non-root directory therefore re-computes one identical decision F + 1
        // times — which is exactly what a first-tier-keyed cache (or a hoist) would collapse to one.
        var (services, store) = BuildProvider(secureMediaEnabled: true);
        var authorizationService = services.GetRequiredService<IAuthorizationService>();

        store.ResetCounters();

        var authorized = await authorizationService.AuthorizeAsync(User(), MediaPermissions.ManageMediaFolder, (object)path);

        Assert.True(authorized, $"Expected '{path}' to inherit the first-tier 'photos' grant.");
        Assert.Equal(1, store.CallsFor(nameof(IFileStore.GetDirectoryInfoAsync)));
    }

    private static async Task<int> MeasureAsync(CountingFileStore store, Func<Task> action)
    {
        store.ResetCounters();
        await action();

        return store.TotalCalls;
    }

    private static ClaimsPrincipal User()
        => new(new ClaimsIdentity([new Claim(ClaimTypes.Name, "editor")], "Test"));

    private static (IServiceProvider Services, CountingFileStore Store) BuildProvider(
        bool secureMediaEnabled,
        string[] grantedPermissions = null)
    {
        // Folder-scoped rights: the user may manage media, and may view only the 'photos' folder.
        // This is the configuration Secure Media exists for, and the one that exercises the full path.
        grantedPermissions ??=
        [
            MediaPermissions.ManageMedia.Name,
            string.Format("ViewMediaContent_{0}", Folder),
        ];

        var store = new CountingFileStore(directories: [Folder, "documents"]);

        var services = new ServiceCollection();

        services.AddOptions();
        services.AddLogging();
        services.Configure<MediaOptions>(options =>
        {
            options.AssetsUsersFolder = "_Users";
            options.AllowedFileExtensions = [".jpg", ".png"];
            options.AssetsRequestPath = "/media";
        });

        services.AddAuthorizationCore();
        services.AddSingleton<IMediaFileStore>(store);
        services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = new DefaultHttpContext() });
        services.AddSingleton<IUserAssetFolderNameProvider, TestUserAssetFolderNameProvider>();
        services.AddSingleton<AttachedMediaFieldFileService>();
        services.AddSingleton<IContentManager>(Mock.Of<IContentManager>());

        // The claims-based grant must run first, mirroring OrchardCore's own permission handler.
        services.AddSingleton<IAuthorizationHandler>(new GrantedPermissionsHandler(grantedPermissions));
        services.AddScoped<IAuthorizationHandler, ManageMediaFolderAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, ViewMediaFolderAuthorizationHandler>();

        if (secureMediaEnabled)
        {
            services.AddSingleton<SecureMediaMarker>();
        }

        return (services.BuildServiceProvider(), store);
    }

    private sealed class TestUserAssetFolderNameProvider : IUserAssetFolderNameProvider
    {
        public string GetUserAssetFolderName(ClaimsPrincipal claimsPrincipal) => "editor";
    }

    /// <summary>
    /// Succeeds a <see cref="PermissionRequirement"/> when the permission (or one it is implied by) was granted.
    /// Stands in for OrchardCore's claims/role based permission handler.
    /// </summary>
    private sealed class GrantedPermissionsHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly HashSet<string> _granted;

        public GrantedPermissionsHandler(IEnumerable<string> granted)
            => _granted = new HashSet<string>(granted, StringComparer.OrdinalIgnoreCase);

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (_granted.Contains(requirement.Permission.Name))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// In-memory <see cref="IMediaFileStore"/> that counts every call, so authorization overhead is
    /// measured rather than estimated.
    /// </summary>
    private sealed class CountingFileStore : IMediaFileStore
    {
        private readonly HashSet<string> _directories;
        private readonly Dictionary<string, int> _counts = [];

        public CountingFileStore(IEnumerable<string> directories)
            => _directories = new HashSet<string>(directories, StringComparer.OrdinalIgnoreCase);

        public int TotalCalls => _counts.Values.Sum();

        public int CallsFor(string method) => _counts.TryGetValue(method, out var count) ? count : 0;

        public void ResetCounters() => _counts.Clear();

        private void Count(string method)
            => _counts[method] = CallsFor(method) + 1;

        public Task<IFileStoreEntry> GetFileInfoAsync(string path)
        {
            Count(nameof(GetFileInfoAsync));

            return Task.FromResult<IFileStoreEntry>(null);
        }

        public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path)
        {
            Count(nameof(GetDirectoryInfoAsync));

            return Task.FromResult<IFileStoreEntry>(
                _directories.Contains(path ?? string.Empty) ? new Entry(path, isDirectory: true) : null);
        }

        public async IAsyncEnumerable<IFileStoreEntry> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false)
        {
            Count(nameof(GetDirectoryContentAsync));

            foreach (var directory in _directories)
            {
                yield return new Entry(directory, isDirectory: true);
            }

            await Task.CompletedTask;
        }

        public async IAsyncEnumerable<IFileStoreEntry> GetFilesAsync(string path = null)
        {
            Count(nameof(GetFilesAsync));

            yield break;
        }

        public async IAsyncEnumerable<IFileStoreEntry> GetDirectoriesAsync(string path = null)
        {
            Count(nameof(GetDirectoriesAsync));

            foreach (var directory in _directories)
            {
                yield return new Entry(directory, isDirectory: true);
            }

            await Task.CompletedTask;
        }

        public IFileStoreCapabilities Capabilities => FileStoreCapabilities.Default;

        public string MapPathToPublicUrl(string path) => "/media/" + path;

        public Task<bool> TryCreateDirectoryAsync(string path) => Task.FromResult(true);

        public Task<bool> TryDeleteFileAsync(string path) => Task.FromResult(true);

        public Task<bool> TryDeleteDirectoryAsync(string path) => Task.FromResult(true);

        public Task MoveFileAsync(string oldPath, string newPath) => Task.CompletedTask;

        public Task CopyFileAsync(string srcPath, string dstPath) => Task.CompletedTask;

        public Task<Stream> GetFileStreamAsync(string path) => Task.FromResult<Stream>(new MemoryStream());

        public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry) => Task.FromResult<Stream>(new MemoryStream());

        public Task<string> CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false)
            => Task.FromResult(path);

        private sealed class Entry : IFileStoreEntry
        {
            public Entry(string path, bool isDirectory)
            {
                Path = path ?? string.Empty;
                IsDirectory = isDirectory;
                var index = Path.LastIndexOf('/');
                Name = index >= 0 ? Path[(index + 1)..] : Path;
                DirectoryPath = index >= 0 ? Path[..index] : string.Empty;
            }

            public string Path { get; }

            public string Name { get; }

            public string DirectoryPath { get; }

            public long Length => 0;

            public DateTime LastModifiedUtc => DateTime.UtcNow;

            public bool IsDirectory { get; }
        }
    }
}
