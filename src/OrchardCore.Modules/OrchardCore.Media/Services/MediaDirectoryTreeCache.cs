using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.FileStorage;

namespace OrchardCore.Media.Services;

/// <summary>
/// Caches the full directory tree in memory. The cache is built lazily on first
/// request and invalidated when folders are created, deleted, or moved.
/// Registered as a singleton per tenant so every request shares the same tree.
/// </summary>
public class MediaDirectoryTreeCache
{
    private readonly IMediaFileStore _mediaFileStore;
    private readonly ILogger<MediaDirectoryTreeCache> _logger;
    private readonly SemaphoreSlim _buildLock = new(1, 1);

    private volatile DirectoryTreeNode _cachedRoot;

    public MediaDirectoryTreeCache(
        IMediaFileStore mediaFileStore,
        ILogger<MediaDirectoryTreeCache> logger)
    {
        _mediaFileStore = mediaFileStore;
        _logger = logger;
    }

    /// <summary>
    /// Returns the cached directory tree, building it on first access.
    /// </summary>
    public async Task<DirectoryTreeNode> GetTreeAsync()
    {
        var root = _cachedRoot;
        if (root != null)
        {
            return root;
        }

        await _buildLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock.
            root = _cachedRoot;
            if (root != null)
            {
                return root;
            }

            root = await BuildTreeAsync();
            _cachedRoot = root;
            return root;
        }
        finally
        {
            _buildLock.Release();
        }
    }

    /// <summary>
    /// Invalidates the cached tree. The next call to <see cref="GetTreeAsync"/>
    /// will rebuild it from the file store.
    /// </summary>
    public void Invalidate()
    {
        _cachedRoot = null;
    }

    private async Task<DirectoryTreeNode> BuildTreeAsync()
    {
        var root = new DirectoryTreeNode
        {
            Name = string.Empty,
            Path = string.Empty,
            Children = [],
        };

        await BuildChildrenAsync(string.Empty, root.Children);
        root.HasChildren = root.Children.Count > 0;

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Built directory tree cache with {Count} top-level folders.", root.Children.Count);
        }

        return root;
    }

    private async Task BuildChildrenAsync(string path, List<DirectoryTreeNode> children)
    {
        await foreach (var entry in _mediaFileStore.GetDirectoriesAsync(path))
        {
            var node = new DirectoryTreeNode
            {
                Name = entry.Name,
                Path = entry.Path,
                Children = [],
            };

            children.Add(node);

            await BuildChildrenAsync(entry.Path, node.Children);
            node.HasChildren = node.Children.Count > 0;
        }
    }
}

/// <summary>
/// Lightweight in-memory representation of a directory tree node.
/// </summary>
public class DirectoryTreeNode
{
    public string Name { get; set; }
    public string Path { get; set; }
    public bool HasChildren { get; set; }
    public List<DirectoryTreeNode> Children { get; set; }
}
