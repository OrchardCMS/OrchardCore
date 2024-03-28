using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OrchardCore.FileStorage;

namespace OrchardCore.Media.Core;

public class NullMediaFileStoreCache : IMediaFileStoreCache
{
    public Task<bool> IsCachedAsync(string path) => Task.FromResult(false);

    public Task<bool> PurgeAsync() => Task.FromResult(false);

    public Task SetCacheAsync(Stream stream, IFileStoreEntry fileStoreEntry, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<bool> TryDeleteDirectoryAsync(string path) => Task.FromResult(false);

    public Task<bool> TryDeleteFileAsync(string path) => Task.FromResult(false);
}
