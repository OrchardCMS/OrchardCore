using System.IO.Pipelines;
using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.FileStorage.AmazonS3;
using OrchardCore.Media.Services;

namespace OrchardCore.Media.AmazonS3.Services;

/// <summary>
/// Amazon S3 implementation of <see cref="ITusTempStore"/>.
/// Uses S3 multipart upload to buffer partial upload chunks across servers.
/// </summary>
public sealed class S3TusTempStore : ITusTempStore
{
    private const string TusTempPrefix = "_tus-uploads";

    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly IDistributedCache _cache;
    private readonly string _tenantId;
    private readonly ILogger _logger;

    public S3TusTempStore(
        IAmazonS3 s3Client,
        IOptions<AwsStorageOptions> options,
        IDistributedCache cache,
        ShellSettings shellSettings,
        ILogger<S3TusTempStore> logger)
    {
        _s3Client = s3Client;
        _bucketName = options.Value.BucketName;
        _cache = cache;
        _tenantId = shellSettings.TenantId;
        _logger = logger;
    }

    public async Task CreateFileAsync(string fileId, CancellationToken cancellationToken)
    {
        var request = new InitiateMultipartUploadRequest
        {
            BucketName = _bucketName,
            Key = GetObjectKey(fileId),
        };

        var response = await _s3Client.InitiateMultipartUploadAsync(request, cancellationToken);

        // Store the multipart upload ID.
        var options = GetCacheOptions();
        await _cache.SetStringAsync(UploadIdKey(fileId), response.UploadId, options, cancellationToken);

        // Initialize empty part list.
        await SavePartListAsync(fileId, [], cancellationToken);
    }

    public async Task<long> AppendDataAsync(string fileId, Stream stream, long offset, CancellationToken cancellationToken)
    {
        var uploadId = await _cache.GetStringAsync(UploadIdKey(fileId), cancellationToken);
        if (string.IsNullOrEmpty(uploadId))
        {
            return 0;
        }

        var parts = await GetPartListAsync(fileId, cancellationToken);
        var partNumber = parts.Count + 1;

        long bytesWritten = 0;
        try
        {
            // Read the entire stream into a buffer for S3 (S3 requires content-length for parts).
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken);
            bytesWritten = memoryStream.Length;

            if (bytesWritten > 0)
            {
                memoryStream.Position = 0;
                var uploadPartRequest = new UploadPartRequest
                {
                    BucketName = _bucketName,
                    Key = GetObjectKey(fileId),
                    UploadId = uploadId,
                    PartNumber = partNumber,
                    InputStream = memoryStream,
                };

                var response = await _s3Client.UploadPartAsync(uploadPartRequest, cancellationToken);
                parts.Add(new PartInfo { PartNumber = partNumber, ETag = response.ETag });
            }
        }
        catch (OperationCanceledException)
        {
            // Client disconnected. Parts already uploaded are durable.
        }

        await SavePartListAsync(fileId, parts, cancellationToken);

        return bytesWritten;
    }

    public async Task<long> AppendDataAsync(string fileId, PipeReader pipeReader, long offset, CancellationToken cancellationToken)
    {
        var uploadId = await _cache.GetStringAsync(UploadIdKey(fileId), cancellationToken);
        if (string.IsNullOrEmpty(uploadId))
        {
            return 0;
        }

        var parts = await GetPartListAsync(fileId, cancellationToken);
        var partNumber = parts.Count + 1;

        long bytesWritten = 0;
        try
        {
            // Buffer the pipe data, then upload as a single part.
            using var memoryStream = new MemoryStream();
            while (true)
            {
                var result = await pipeReader.ReadAsync(cancellationToken);
                var buffer = result.Buffer;

                foreach (var segment in buffer)
                {
                    memoryStream.Write(segment.Span);
                    bytesWritten += segment.Length;
                }

                pipeReader.AdvanceTo(buffer.End);

                if (result.IsCompleted)
                {
                    break;
                }
            }

            if (bytesWritten > 0)
            {
                memoryStream.Position = 0;
                var uploadPartRequest = new UploadPartRequest
                {
                    BucketName = _bucketName,
                    Key = GetObjectKey(fileId),
                    UploadId = uploadId,
                    PartNumber = partNumber,
                    InputStream = memoryStream,
                };

                var response = await _s3Client.UploadPartAsync(uploadPartRequest, cancellationToken);
                parts.Add(new PartInfo { PartNumber = partNumber, ETag = response.ETag });
            }
        }
        catch (OperationCanceledException)
        {
            // Client disconnected. Parts already uploaded are durable.
        }

        await SavePartListAsync(fileId, parts, cancellationToken);

        return bytesWritten;
    }

    public async Task DeleteFileAsync(string fileId, CancellationToken cancellationToken)
    {
        try
        {
            var uploadId = await _cache.GetStringAsync(UploadIdKey(fileId), cancellationToken);

            // Abort any in-progress multipart upload.
            if (!string.IsNullOrEmpty(uploadId))
            {
                try
                {
                    await _s3Client.AbortMultipartUploadAsync(
                        _bucketName, GetObjectKey(fileId), uploadId, cancellationToken);
                }
                catch
                {
                    // Best effort — upload may already be completed or aborted.
                }
            }

            // Delete the committed object if it exists.
            await _s3Client.DeleteObjectAsync(_bucketName, GetObjectKey(fileId), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete TUS temp S3 object for file '{FileId}'.", fileId);
        }

        await _cache.RemoveAsync(UploadIdKey(fileId), cancellationToken);
        await _cache.RemoveAsync(PartListKey(fileId), cancellationToken);
    }

    public async Task<bool> FileExistAsync(string fileId, CancellationToken cancellationToken)
    {
        // Check if we have an active multipart upload.
        var uploadId = await _cache.GetStringAsync(UploadIdKey(fileId), cancellationToken);
        if (!string.IsNullOrEmpty(uploadId))
        {
            return true;
        }

        // Check for a completed object.
        try
        {
            await _s3Client.GetObjectMetadataAsync(_bucketName, GetObjectKey(fileId), cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public Stream OpenReadStream(string fileId)
    {
        // Complete the multipart upload first if needed.
        CompleteMultipartUploadIfNeeded(fileId);

        var response = _s3Client.GetObjectAsync(_bucketName, GetObjectKey(fileId)).GetAwaiter().GetResult();
        return response.ResponseStream;
    }

    public long GetFileLength(string fileId)
    {
        // Complete the multipart upload first if needed.
        CompleteMultipartUploadIfNeeded(fileId);

        var metadata = _s3Client.GetObjectMetadataAsync(_bucketName, GetObjectKey(fileId))
            .GetAwaiter().GetResult();
        return metadata.ContentLength;
    }

    // Helpers

    private string GetObjectKey(string fileId) => $"{TusTempPrefix}/{_tenantId}/{fileId}";
    private string UploadIdKey(string fileId) => $"tus:s3upload:{_tenantId}:{fileId}";
    private string PartListKey(string fileId) => $"tus:s3parts:{_tenantId}:{fileId}";

    private void CompleteMultipartUploadIfNeeded(string fileId)
    {
        var uploadId = _cache.GetStringAsync(UploadIdKey(fileId)).GetAwaiter().GetResult();
        if (string.IsNullOrEmpty(uploadId))
        {
            return;
        }

        var parts = GetPartListAsync(fileId, CancellationToken.None).GetAwaiter().GetResult();
        if (parts.Count == 0)
        {
            return;
        }

        var request = new CompleteMultipartUploadRequest
        {
            BucketName = _bucketName,
            Key = GetObjectKey(fileId),
            UploadId = uploadId,
            PartETags = parts.Select(p => new PartETag(p.PartNumber, p.ETag)).ToList(),
        };

        _s3Client.CompleteMultipartUploadAsync(request).GetAwaiter().GetResult();

        // Clean up cache entries for the multipart state.
        _cache.RemoveAsync(UploadIdKey(fileId)).GetAwaiter().GetResult();
        _cache.RemoveAsync(PartListKey(fileId)).GetAwaiter().GetResult();
    }

    private static DistributedCacheEntryOptions GetCacheOptions()
    {
        return new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromHours(24),
        };
    }

    private async Task<List<PartInfo>> GetPartListAsync(string fileId, CancellationToken cancellationToken)
    {
        var json = await _cache.GetStringAsync(PartListKey(fileId), cancellationToken);
        if (string.IsNullOrEmpty(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<PartInfo>>(json) ?? [];
    }

    private async Task SavePartListAsync(string fileId, List<PartInfo> parts, CancellationToken cancellationToken)
    {
        await _cache.SetStringAsync(
            PartListKey(fileId),
            JsonSerializer.Serialize(parts),
            GetCacheOptions(),
            cancellationToken);
    }

    private sealed class PartInfo
    {
        public int PartNumber { get; set; }
        public string ETag { get; set; }
    }
}
