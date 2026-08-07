using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.FileStorage;
using OrchardCore.Media;
using OrchardCore.Media.Endpoints.Api;
using OrchardCore.Media.Services;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

/// <summary>
/// Listing a directory reports, for each folder it contains, whether that folder has sub-folders — the
/// flag that draws the expand arrow. Answering it by probing the store costs one round-trip per folder,
/// a network call each on a remote store, to re-derive what the cached directory tree already holds.
///
/// The tree is permission-agnostic though, so these tests pin down where it may be trusted and where a
/// probe is still required: reporting a folder as having children when the user cannot see any of them
/// would disclose their existence.
/// </summary>
public class MediaHasChildrenCostTests
{
    [Fact]
    public async Task AnEmptyFolder_IsAnsweredFromTheTree_WithoutProbing()
    {
        // "No sub-directories at all" cannot be changed by permissions — there is nothing to deny — so
        // the tree answers it for any user, however restricted.
        var (store, treeCache) = BuildStore("photos", "photos/2026");

        await treeCache.GetTreeAsync();
        store.ResetCounters();

        var hasChildren = await MediaEndpointHelpers.HasSubDirectoriesAsync(
            store, DenyAll(), User(), "photos/2026", treeCache: treeCache);

        Assert.False(hasChildren);
        Assert.Equal(0, store.TotalCalls);
    }

    [Fact]
    public async Task AFolderWithChildren_IsAnsweredFromTheTree_WhenEveryFolderIsVisible()
    {
        var (store, treeCache) = BuildStore("photos", "photos/2026");

        await treeCache.GetTreeAsync();
        store.ResetCounters();

        var hasChildren = await MediaEndpointHelpers.HasSubDirectoriesAsync(
            store, GrantAll(), User(), "photos", treeCache: treeCache, everyFolderIsVisible: true);

        Assert.True(hasChildren);
        Assert.Equal(0, store.TotalCalls);
    }

    [Fact]
    public async Task AFolderWithChildren_IsStillProbed_WhenTheUserIsRestricted()
    {
        // This is the security property. The tree knows 'photos' has a child; it does not know whether
        // this user may see it. Reporting true from the tree would draw an expand arrow for a folder
        // whose contents are all denied, disclosing that something is there.
        var (store, treeCache) = BuildStore("photos", "photos/2026");

        await treeCache.GetTreeAsync();
        store.ResetCounters();

        var hasChildren = await MediaEndpointHelpers.HasSubDirectoriesAsync(
            store, DenyAll(), User(), "photos", treeCache: treeCache, everyFolderIsVisible: false);

        Assert.False(hasChildren);
        Assert.True(store.TotalCalls > 0, "A restricted user must still be checked against the store.");
    }

    [Fact]
    public async Task AFolderTheTreeDoesNotKnow_FallsBackToProbing()
    {
        // Created outside the media API, so the cached tree has never seen it. Answering "no children"
        // would be wrong; the probe is the safe fallback.
        var (store, treeCache) = BuildStore("photos");

        await treeCache.GetTreeAsync();

        store.Add("out-of-band", "out-of-band/child");
        store.ResetCounters();

        var hasChildren = await MediaEndpointHelpers.HasSubDirectoriesAsync(
            store, GrantAll(), User(), "out-of-band", treeCache: treeCache, everyFolderIsVisible: true);

        Assert.True(hasChildren);
        Assert.True(store.TotalCalls > 0);
    }

    private static ClaimsPrincipal User()
        => new(new ClaimsIdentity([new Claim(ClaimTypes.Name, "editor")], "Test"));

    private static IAuthorizationService GrantAll() => new StubAuthorizationService(true);

    private static IAuthorizationService DenyAll() => new StubAuthorizationService(false);

    private static (CountingDirectoryStore Store, MediaDirectoryTreeCache TreeCache) BuildStore(params string[] directories)
    {
        var store = new CountingDirectoryStore(directories);

        return (store, new MediaDirectoryTreeCache(store, NullLogger<MediaDirectoryTreeCache>.Instance));
    }

    private sealed class StubAuthorizationService : IAuthorizationService
    {
        private readonly bool _granted;

        public StubAuthorizationService(bool granted) => _granted = granted;

        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object resource, IEnumerable<IAuthorizationRequirement> requirements)
            => Task.FromResult(_granted ? AuthorizationResult.Success() : AuthorizationResult.Failed());

        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object resource, string policyName)
            => Task.FromResult(_granted ? AuthorizationResult.Success() : AuthorizationResult.Failed());
    }

    /// <summary>
    /// In-memory directory-only store that counts enumerations, so probes are measured rather than assumed.
    /// </summary>
    private sealed class CountingDirectoryStore : IMediaFileStore
    {
        private readonly List<string> _directories;

        public CountingDirectoryStore(IEnumerable<string> directories) => _directories = [.. directories];

        public int TotalCalls { get; private set; }

        public void ResetCounters() => TotalCalls = 0;

        public void Add(params string[] directories) => _directories.AddRange(directories);

        public async IAsyncEnumerable<IFileStoreEntry> GetDirectoriesAsync(string path = null)
        {
            TotalCalls++;

            var parent = path ?? string.Empty;

            foreach (var directory in _directories)
            {
                var directoryParent = directory.Contains('/') ? directory[..directory.LastIndexOf('/')] : string.Empty;

                if (string.Equals(directoryParent, parent, StringComparison.Ordinal))
                {
                    yield return new Entry(directory);
                }
            }

            await Task.CompletedTask;
        }

        public async IAsyncEnumerable<IFileStoreEntry> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false)
        {
            await foreach (var entry in GetDirectoriesAsync(path))
            {
                yield return entry;
            }
        }

        public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path)
            => Task.FromResult<IFileStoreEntry>(_directories.Contains(path) ? new Entry(path) : null);

        public Task<IFileStoreEntry> GetFileInfoAsync(string path) => Task.FromResult<IFileStoreEntry>(null);

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
            public Entry(string path)
            {
                Path = path;
                var index = path.LastIndexOf('/');
                Name = index >= 0 ? path[(index + 1)..] : path;
                DirectoryPath = index >= 0 ? path[..index] : string.Empty;
            }

            public string Path { get; }

            public string Name { get; }

            public string DirectoryPath { get; }

            public long Length => 0;

            public DateTime LastModifiedUtc => DateTime.UtcNow;

            public bool IsDirectory => true;
        }
    }
}
