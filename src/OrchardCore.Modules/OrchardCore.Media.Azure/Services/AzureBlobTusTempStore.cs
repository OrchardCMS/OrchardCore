using System.IO.Pipelines;
using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Media.Services;

namespace OrchardCore.Media.Azure.Services;

/// <summary>
/// Azure Blob Storage implementation of <see cref="ITusTempStore"/>.
/// Uses Block Blob staging (StageBlockAsync) to buffer
/// partial upload chunks, then commits them on read (CommitBlockListAsync).
/// </summary>
public sealed class AzureBlobTusTempStore : ITusTempStore
{
    private const string TusTempPrefix = "_tus-uploads";

    private readonly BlobContainerClient _containerClient;
    private readonly IDistributedCache _cache;
    private readonly string _tenantId;
    private readonly ILogger _logger;

    public AzureBlobTusTempStore(
        IOptions<MediaBlobStorageOptions> options,
        IDistributedCache cache,
        ShellSettings shellSettings,
        ILogger<AzureBlobTusTempStore> logger)
    {
        var opts = options.Value;
        _containerClient = new BlobContainerClient(opts.ConnectionString, opts.ContainerName);
        _cache = cache;
        _tenantId = shellSettings.TenantId;
        _logger = logger;
    }

    public Task CreateFileAsync(string fileId, CancellationToken cancellationToken)
    {
        // No-op for Azure — blocks are staged on append, and the blob is created implicitly.
        return Task.CompletedTask;
    }

    public async Task<long> AppendDataAsync(string fileId, Stream stream, long offset, CancellationToken cancellationToken)
    {
        var blockBlob = GetBlockBlobClient(fileId);
        var blockIds = await GetBlockListAsync(fileId, cancellationToken);

        long bytesWritten = 0;
        try
        {
            // Read the incoming stream in chunks and stage each as a block.
            var buffer = new byte[4 * 1024 * 1024]; // 4 MB blocks
            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(), cancellationToken)) > 0)
            {
                var blockId = GenerateBlockId(blockIds.Count);
                using var blockStream = new MemoryStream(buffer, 0, bytesRead);
                await blockBlob.StageBlockAsync(blockId, blockStream, cancellationToken: cancellationToken);
                blockIds.Add(blockId);
                bytesWritten += bytesRead;
            }
        }
        catch (OperationCanceledException)
        {
            // Client disconnected. Blocks already staged are durable.
        }

        await SaveBlockListAsync(fileId, blockIds, cancellationToken);

        return bytesWritten;
    }

    public async Task<long> AppendDataAsync(string fileId, PipeReader pipeReader, long offset, CancellationToken cancellationToken)
    {
        var blockBlob = GetBlockBlobClient(fileId);
        var blockIds = await GetBlockListAsync(fileId, cancellationToken);

        long bytesWritten = 0;
        try
        {
            while (true)
            {
                var result = await pipeReader.ReadAsync(cancellationToken);
                var buffer = result.Buffer;

                foreach (var segment in buffer)
                {
                    if (segment.Length == 0)
                    {
                        continue;
                    }

                    var blockId = GenerateBlockId(blockIds.Count);
                    using var blockStream = new MemoryStream(segment.ToArray());
                    await blockBlob.StageBlockAsync(blockId, blockStream, cancellationToken: cancellationToken);
                    blockIds.Add(blockId);
                    bytesWritten += segment.Length;
                }

                pipeReader.AdvanceTo(buffer.End);

                if (result.IsCompleted)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Client disconnected. Blocks already staged are durable.
        }

        await SaveBlockListAsync(fileId, blockIds, cancellationToken);

        return bytesWritten;
    }

    public async Task DeleteFileAsync(string fileId, CancellationToken cancellationToken)
    {
        try
        {
            var blockBlob = GetBlockBlobClient(fileId);
            await blockBlob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete TUS temp blob for file '{FileId}'.", fileId);
        }

        await _cache.RemoveAsync(BlockListKey(fileId), cancellationToken);
    }

    public async Task<bool> FileExistAsync(string fileId, CancellationToken cancellationToken)
    {
        // Check if we have staged blocks (uncommitted blob won't show as "exists").
        var blockIds = await GetBlockListAsync(fileId, cancellationToken);
        if (blockIds.Count > 0)
        {
            return true;
        }

        // Also check for a committed blob.
        var blockBlob = GetBlockBlobClient(fileId);
        var response = await blockBlob.ExistsAsync(cancellationToken);
        return response.Value;
    }

    public Stream OpenReadStream(string fileId)
    {
        // Commit all staged blocks first, then open for reading.
        var blockIds = GetBlockListAsync(fileId, CancellationToken.None).GetAwaiter().GetResult();
        var blockBlob = GetBlockBlobClient(fileId);

        if (blockIds.Count > 0)
        {
            blockBlob.CommitBlockListAsync(blockIds).GetAwaiter().GetResult();
        }

        return blockBlob.OpenRead();
    }

    public long GetFileLength(string fileId)
    {
        var blockBlob = GetBlockBlobClient(fileId);
        var properties = blockBlob.GetProperties();
        return properties.Value.ContentLength;
    }

    // Helpers

    private BlockBlobClient GetBlockBlobClient(string fileId)
    {
        var blobName = $"{TusTempPrefix}/{_tenantId}/{fileId}";
        return _containerClient.GetBlockBlobClient(blobName);
    }

    private static string GenerateBlockId(int index)
    {
        // Block IDs must be the same length and Base64 encoded.
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(index.ToString("D8")));
    }

    private string BlockListKey(string fileId) => $"tus:blocks:{_tenantId}:{fileId}";

    private async Task<List<string>> GetBlockListAsync(string fileId, CancellationToken cancellationToken)
    {
        var json = await _cache.GetStringAsync(BlockListKey(fileId), cancellationToken);
        if (string.IsNullOrEmpty(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<string>>(json) ?? [];
    }

    private async Task SaveBlockListAsync(string fileId, List<string> blockIds, CancellationToken cancellationToken)
    {
        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromHours(24),
        };

        await _cache.SetStringAsync(
            BlockListKey(fileId),
            JsonSerializer.Serialize(blockIds),
            options,
            cancellationToken);
    }
}
