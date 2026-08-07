using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Caching;
using OrchardCore.ContentManagement;
using OrchardCore.FileStorage;
using OrchardCore.Media;
using OrchardCore.Media.Services;
using OrchardCore.Environment.Cache;
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
    public async Task TheCacheNeverCachesAnAuthorizationDecision()
    {
        // The reason this cache needs no invalidation when folder permissions change: it holds facts about
        // the file store — a path's canonical form, and whether it is an existing directory — never who may
        // see it. Two users sharing one cache must therefore still get their own decisions. If this ever
        // fails, the cache has started remembering a decision and has become a cross-user leak.
        var (services, _) = BuildProvider(
            secureMediaEnabled: true,
            grantedPermissions: [MediaPermissions.ManageMedia.Name, "ViewMediaContent_photos"],
            grantsByUser: handler => handler
                .GrantTo("alpha", MediaPermissions.ManageMedia.Name, "ViewMediaContent_photos")
                .GrantTo("beta", MediaPermissions.ManageMedia.Name, "ViewMediaContent_documents"));

        var authorizationService = services.GetRequiredService<IAuthorizationService>();

        // Alpha goes first and populates the cache for both paths.
        var alphaOnPhotos = await authorizationService.AuthorizeAsync(User("alpha"), MediaPermissions.ManageMediaFolder, (object)"photos");
        var alphaOnDocuments = await authorizationService.AuthorizeAsync(User("alpha"), MediaPermissions.ManageMediaFolder, (object)"documents");

        // Beta then reuses that cache and must still be judged on its own grants.
        var betaOnPhotos = await authorizationService.AuthorizeAsync(User("beta"), MediaPermissions.ManageMediaFolder, (object)"photos");
        var betaOnDocuments = await authorizationService.AuthorizeAsync(User("beta"), MediaPermissions.ManageMediaFolder, (object)"documents");

        Assert.True(alphaOnPhotos);
        Assert.False(alphaOnDocuments);
        Assert.False(betaOnPhotos);
        Assert.True(betaOnDocuments);
    }

    [Fact]
    public async Task AMissingPath_CostsABoundedNumberOfProbes()
    {
        // The path is supplied by the caller, and anchoring one that does not exist probes its ancestors
        // one at a time. Without a bound, a request naming a deeply nested path would buy hundreds of
        // file-store round-trips for the price of one HTTP request.
        var (services, store) = BuildProvider(secureMediaEnabled: false);
        var authorizationService = services.GetRequiredService<IAuthorizationService>();

        var shallow = string.Join('/', Enumerable.Repeat("missing", 4));
        var absurd = string.Join('/', Enumerable.Repeat("missing", 200));

        store.ResetCounters();
        await authorizationService.AuthorizeAsync(User(), MediaPermissions.ManageMediaFolder, (object)shallow);
        var shallowCost = store.TotalCalls;

        store.ResetCounters();
        await authorizationService.AuthorizeAsync(User(), MediaPermissions.ManageMediaFolder, (object)absurd);
        var absurdCost = store.TotalCalls;

        Assert.True(shallowCost > 0);

        // Fifty times the depth must not buy fifty times the work.
        Assert.True(absurdCost < 25,
            $"A 200-segment path cost {absurdCost} file-store calls; the walk is expected to be bounded.");
    }

    [Fact]
    public async Task FolderScopedGrant_AuthorizesTheRootFolder()
    {
        // https://github.com/OrchardCMS/OrchardCore/issues/19675
        // SecureMediaPermissions publishes a ViewRootMediaContent permission implied by every first-level
        // folder permission, so holding ViewMediaContent_photos is meant to imply root access. The handler
        // used to authorize against the static instance instead, which carries no such implication, and a
        // folder-scoped role was denied at the root while the role editor showed it as allowed.
        var (services, _) = BuildProvider(
            secureMediaEnabled: true,
            grantedPermissions: [MediaPermissions.ManageMedia.Name, "ViewMediaContent_photos"]);

        var authorizationService = services.GetRequiredService<IAuthorizationService>();

        var authorized = await authorizationService.AuthorizeAsync(User(), MediaPermissions.ManageMediaFolder, (object)string.Empty);

        Assert.True(authorized, "A folder-scoped grant is documented to imply root access.");
    }

    [Fact]
    public async Task MarkingAPathAsAnExistingDirectory_GrantsNothing()
    {
        // Seeding is an assertion about the file store, not a grant. A folder the user cannot view stays
        // denied even once it is declared canonical — otherwise the optimization would be a way to bypass
        // authorization.
        var (services, _) = BuildProvider(secureMediaEnabled: true);
        var authorizationService = services.GetRequiredService<IAuthorizationService>();

        services.GetRequiredService<MediaPathResolutionCache>().MarkExistingDirectory("documents");

        var authorized = await authorizationService.AuthorizeAsync(User(), MediaPermissions.ManageMediaFolder, (object)"documents");

        Assert.False(authorized, "Only 'photos' was granted, so 'documents' must stay denied.");
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

    private static ClaimsPrincipal User(string name = "editor")
        => new(new ClaimsIdentity([new Claim(ClaimTypes.Name, name)], "Test"));

    private static (IServiceProvider Services, CountingFileStore Store) BuildProvider(
        bool secureMediaEnabled,
        string[] grantedPermissions = null,
        Func<GrantedPermissionsHandler, GrantedPermissionsHandler> grantsByUser = null)
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
        services.AddMemoryCache();
        services.AddScoped<MediaPathResolutionCache>();
        services.AddSingleton<IMediaFileStore>(store);
        services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = new DefaultHttpContext() });
        services.AddSingleton<IUserAssetFolderNameProvider, TestUserAssetFolderNameProvider>();
        services.AddSingleton<AttachedMediaFieldFileService>();
        services.AddSingleton<IContentManager>(Mock.Of<IContentManager>());

        // The claims-based grant must run first, mirroring OrchardCore's own permission handler.
        var permissionHandler = new GrantedPermissionsHandler(grantedPermissions);
        grantsByUser?.Invoke(permissionHandler);
        services.AddSingleton<IAuthorizationHandler>(permissionHandler);
        services.AddScoped<IAuthorizationHandler, ManageMediaFolderAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, ViewMediaFolderAuthorizationHandler>();

        if (secureMediaEnabled)
        {
            services.AddSingleton<SecureMediaMarker>();

            // The provider publishes the ViewRootMediaContent permission that first-level folder
            // permissions imply. The handler must authorize against that instance, not the static one.
            services.AddSingleton<ISignal, Signal>();
            services.AddSingleton<IPermissionProvider, SecureMediaPermissions>();
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
        private readonly Dictionary<string, HashSet<string>> _grantedByUser;

        public GrantedPermissionsHandler(IEnumerable<string> granted)
        {
            _granted = new HashSet<string>(granted, StringComparer.OrdinalIgnoreCase);
            _grantedByUser = [];
        }

        /// <summary>
        /// Grants a different set to a specific user, so a single scope can serve more than one principal.
        /// </summary>
        public GrantedPermissionsHandler GrantTo(string userName, params string[] permissions)
        {
            _grantedByUser[userName] = new HashSet<string>(permissions, StringComparer.OrdinalIgnoreCase);

            return this;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var userName = context.User?.Identity?.Name;

            var granted = userName is not null && _grantedByUser.TryGetValue(userName, out var forUser)
                ? forUser
                : _granted;

            // OrchardCore's own handler grants a permission when any permission that implies it is held,
            // so the stub has to walk ImpliedBy too — otherwise implications cannot be tested at all.
            if (IsGranted(requirement.Permission, granted, []))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        private static bool IsGranted(Permission permission, HashSet<string> granted, HashSet<string> visited)
        {
            if (permission is null || !visited.Add(permission.Name))
            {
                return false;
            }

            if (granted.Contains(permission.Name))
            {
                return true;
            }

            foreach (var impliedBy in permission.ImpliedBy ?? [])
            {
                if (IsGranted(impliedBy, granted, visited))
                {
                    return true;
                }
            }

            return false;
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
