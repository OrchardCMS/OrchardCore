#nullable enable

using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.FileStorage.AmazonS3;
using OrchardCore.Media;

namespace OrchardCore.Media.AmazonS3.Services;

internal sealed class AWSS3ResizedImageCache : IResizedImageCache
{
    private readonly AwsMediaImageCacheOptions _options;
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger _logger;

    public AWSS3ResizedImageCache(
        IOptions<AwsMediaImageCacheOptions> options,
        IAmazonS3 s3Client,
        ILogger<AWSS3ResizedImageCache> logger)
    {
        _options = options.Value;
        _s3Client = s3Client;
        _logger = logger;
    }

    public async Task<(Stream Content, string ContentType)?> GetAsync(string cacheKey, string fileExtension, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_options.BucketName))
        {
            return null;
        }

        var key = GetObjectKey(cacheKey, fileExtension);
        try
        {
            var response = await _s3Client.GetObjectAsync(_options.BucketName, key, cancellationToken);

            return (response.ResponseStream, MediaResizingConstants.ExtensionToContentType(fileExtension));
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Not found — cache miss.
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error reading S3 cached object '{Key}'.", key);
        }

        return null;
    }

    public async Task SetAsync(string cacheKey, Stream image, string contentType, TimeSpan maxAge, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_options.BucketName))
        {
            return;
        }

        var ext = MediaResizingConstants.ContentTypeToExtension(contentType);
        var key = GetObjectKey(cacheKey, ext);

        try
        {
            var request = new PutObjectRequest
            {
                BucketName = _options.BucketName,
                Key = key,
                InputStream = image,
                ContentType = contentType,
                AutoCloseStream = false,
            };

            await _s3Client.PutObjectAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error writing S3 cached object '{Key}'.", key);
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_options.BucketName))
        {
            return;
        }

        try
        {
            var prefix = string.IsNullOrEmpty(_options.BasePath) ? string.Empty : _options.BasePath.TrimEnd('/') + '/';
            string? continuationToken = null;

            do
            {
                var listRequest = new ListObjectsV2Request
                {
                    BucketName = _options.BucketName,
                    Prefix = prefix,
                    ContinuationToken = continuationToken,
                };

                var listResponse = await _s3Client.ListObjectsV2Async(listRequest, cancellationToken);

                if (listResponse.S3Objects.Count > 0)
                {
                    var deleteRequest = new DeleteObjectsRequest
                    {
                        BucketName = _options.BucketName,
                        Objects = listResponse.S3Objects.Select(o => new KeyVersion { Key = o.Key }).ToList(),
                    };

                    await _s3Client.DeleteObjectsAsync(deleteRequest, cancellationToken);
                }

                continuationToken = listResponse.IsTruncated == true ? listResponse.NextContinuationToken : null;
            }
            while (continuationToken != null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error clearing S3 resized image cache.");
        }
    }

    public async Task ClearStaleAsync(TimeSpan maxAge, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_options.BucketName))
        {
            return;
        }

        var cutoff = DateTime.UtcNow - maxAge;
        var prefix = string.IsNullOrEmpty(_options.BasePath) ? string.Empty : _options.BasePath.TrimEnd('/') + '/';
        string? continuationToken = null;

        try
        {
            do
            {
                var listRequest = new ListObjectsV2Request
                {
                    BucketName = _options.BucketName,
                    Prefix = prefix,
                    ContinuationToken = continuationToken,
                };

                var listResponse = await _s3Client.ListObjectsV2Async(listRequest, cancellationToken);

                var staleObjects = listResponse.S3Objects
                    .Where(o => o.LastModified < cutoff)
                    .Select(o => new KeyVersion { Key = o.Key })
                    .ToList();

                if (staleObjects.Count > 0)
                {
                    var deleteRequest = new DeleteObjectsRequest
                    {
                        BucketName = _options.BucketName,
                        Objects = staleObjects,
                    };

                    await _s3Client.DeleteObjectsAsync(deleteRequest, cancellationToken);
                }

                continuationToken = listResponse.IsTruncated == true ? listResponse.NextContinuationToken : null;
            }
            while (continuationToken != null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error clearing stale S3 resized image cache entries.");
        }
    }

    private string GetObjectKey(string cacheKey, string extension)
    {
        var prefix = string.IsNullOrEmpty(_options.BasePath) ? string.Empty : _options.BasePath.TrimEnd('/') + '/';
        var subDir = cacheKey.Length >= 2 ? cacheKey[..2] : "xx";
        return $"{prefix}{subDir}/{cacheKey}{extension}";
    }
}
