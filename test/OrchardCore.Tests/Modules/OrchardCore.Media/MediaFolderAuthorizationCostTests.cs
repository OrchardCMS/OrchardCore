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
    public async Task EnumeratedFolders_CostNothingToAuthorize()
    {
        // The whole point of the fix: a directory listing authorizes every folder it contains, and those
        // paths came from the store's own enumeration. Declaring them as such means neither resolution
        // nor the directory probe has to run — so the listing's authorization cost is zero round-trips,
        // however many folders it holds.
        const int F = 10;

        var (services, store) = BuildProvider(secureMediaEnabled: true);
        var authorizationService = services.GetRequiredService<IAuthorizationService>();
        var pathCache = services.GetRequiredService<MediaPathResolutionCache>();

        store.ResetCounters();

        for (var i = 0; i <= F; i++)
        {
            // What MediaEndpointHelpers does for each entry it enumerates.
            pathCache.MarkExistingDirectory(Folder);

            await authorizationService.AuthorizeAsync(User(), MediaPermissions.ManageMediaFolder, (object)Folder);
        }

        Assert.Equal(0, store.TotalCalls);
    }

    [Fact]
    public async Task EnumeratedFolders_ReachTheSameDecisionAsResolvedOnes()
    {
        // Declaring a path canonical must not change who is authorized, only what it costs to find out.
        foreach (var path in new[] { Folder, "photos/2026", "documents", "documents/private" })
        {
            var (resolvedServices, _) = BuildProvider(secureMediaEnabled: true);
            var resolved = await resolvedServices.GetRequiredService<IAuthorizationService>()
                .AuthorizeAsync(User(), MediaPermissions.ManageMediaFolder, (object)path);

            var (seededServices, _) = BuildProvider(secureMediaEnabled: true);
            seededServices.GetRequiredService<MediaPathResolutionCache>().MarkExistingDirectory(path);
            var seeded = await seededServices.GetRequiredService<IAuthorizationService>()
                .AuthorizeAsync(User(), MediaPermissions.ManageMediaFolder, (object)path);

            Assert.Equal(resolved, seeded);
        }
    }

    [Fact]
    public async Task RepeatedChecksOfTheSamePath_ResolveItOnce()
    {
        // Within one request the resolution of a path cannot change, so authorizing the same folder again
        // must be free. Before the request-scoped cache this cost a full resolution every time.
        const int Repeats = 10;

        var (services, store) = BuildProvider(secureMediaEnabled: false);
        var authorizationService = services.GetRequiredService<IAuthorizationService>();

        store.ResetCounters();
        await authorizationService.AuthorizeAsync(User(), MediaPermissions.ManageMediaFolder, (object)Folder);
        var firstCheck = store.TotalCalls;

        store.ResetCounters();

        for (var i = 0; i < Repeats; i++)
        {
            await authorizationService.AuthorizeAsync(User(), MediaPermissions.ManageMediaFolder, (object)Folder);
        }

        Assert.True(firstCheck > 0, "The first check must actually resolve the path.");
        Assert.Equal(0, store.TotalCalls);
    }

    [Fact]
    public async Task TheCacheDoesNotChangeWhoIsAuthorized()
    {
        // The optimization must only change what a decision costs, never the decision.
        var (services, _) = BuildProvider(secureMediaEnabled: true);
        var authorizationService = services.GetRequiredService<IAuthorizationService>();

        foreach (var path in new[] { Folder, "photos/2026", "documents", "documents/private" })
        {
            var first = await authorizationService.AuthorizeAsync(User(), MediaPermissions.ManageMediaFolder, (object)path);
            var second = await authorizationService.AuthorizeAsync(User(), MediaPermissions.ManageMediaFolder, (object)path);

            Assert.Equal(first, second);
        }
    }

    [Fact]
    public async Task ManageMediaFolder_FromAString_PaysForPathResolution()
    {
        var (services, store) = BuildProvider(secureMediaEnabled: false);
        var authorizationService = services.GetRequiredService<IAuthorizationService>();

        store.ResetCounters();

        var authorized = await authorizationService.AuthorizeAsync(User(), MediaPermissions.ManageMediaFolder, (object)Folder);

        Assert.True(authorized);

        // A request-supplied path must be resolved against the store before it can be trusted, which
        // costs round-trips. This is the price of path-traversal hardening, and it is why callers that
        // already hold a canonical path should say so.
        Assert.True(store.TotalCalls > 0);
    }

    [Fact]
    public async Task ManageMediaFolder_WithSecureMedia_ResolvesThePathOnlyOnce()
    {
        var withoutSecureMedia = await MeasureSingleCheckAsync(secureMediaEnabled: false, Folder);
        var withSecureMedia = await MeasureSingleCheckAsync(secureMediaEnabled: true, Folder);

        // Secure Media evaluates ViewMedia for the same path. Thanks to the request-scoped cache that
        // second evaluation reuses the resolution, so it adds only its own directory probe — not another
        // full resolution, which would have doubled the cost.
        Assert.Equal(withoutSecureMedia + 1, withSecureMedia);
    }

    [Fact]
    public async Task ManageMediaFolder_RepeatedChecks_CostLessThanLinear()
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

        // Without the request-scoped cache this was strictly linear: perCheck x (F + 1). The resolution
        // is now paid once, leaving only the per-check work that the cache cannot remove.
        Assert.True(forListing < perCheck * (F + 1),
            $"Expected repeated checks to cost less than linear ({forListing} vs {perCheck * (F + 1)}).");
    }

    [Fact]
    public async Task ManageMediaFolder_WithGlobalPermission_StillPaysToResolveAString()
    {
        // Holding broad permissions does not avoid the cost: a request-supplied path is resolved before
        // any permission is consulted. Only passing a canonical path avoids it — which is why the fix
        // acts on the resource type rather than on the permissions.
        var (services, store) = BuildProvider(
            secureMediaEnabled: true,
            grantedPermissions: [MediaPermissions.ManageMedia.Name, MediaPermissions.ViewMedia.Name]);

        var authorizationService = services.GetRequiredService<IAuthorizationService>();

        store.ResetCounters();
        var authorized = await authorizationService.AuthorizeAsync(User(), MediaPermissions.ManageMediaFolder, (object)Folder);

        Assert.True(authorized);
        Assert.True(store.TotalCalls > 0);
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
        var (services, _) = BuildProvider(secureMediaEnabled: true);
        var authorizationService = services.GetRequiredService<IAuthorizationService>();

        var authorized = await authorizationService.AuthorizeAsync(User(), MediaPermissions.ManageMediaFolder, (object)path);

        Assert.True(authorized, $"Expected '{path}' to inherit the first-tier 'photos' grant.");
    }

    private static async Task<int> MeasureSingleCheckAsync(bool secureMediaEnabled, string path)
    {
        var (services, store) = BuildProvider(secureMediaEnabled);
        var authorizationService = services.GetRequiredService<IAuthorizationService>();

        store.ResetCounters();

        await authorizationService.AuthorizeAsync(User(), MediaPermissions.ManageMediaFolder, (object)path);

        return store.TotalCalls;
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
        services.AddScoped<MediaPathResolutionCache>();
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
