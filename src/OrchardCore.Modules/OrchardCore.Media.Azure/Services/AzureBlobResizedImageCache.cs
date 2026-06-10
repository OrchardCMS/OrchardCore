#nullable enable

using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Media;

namespace OrchardCore.Media.Azure.Services;

internal sealed class AzureBlobResizedImageCache : IResizedImageCache
{
    private static readonly (string Extension, string ContentType)[] _formats =
    [
        (".jpg",  "image/jpeg"),
        (".png",  "image/png"),
        (".webp", "image/webp"),
        (".gif",  "image/gif"),
        (".bmp",  "image/bmp"),
    ];

    private readonly ImageSharpBlobImageCacheOptions _options;
    private readonly ILogger _logger;

    public AzureBlobResizedImageCache(
        IOptions<ImageSharpBlobImageCacheOptions> options,
        ILogger<AzureBlobResizedImageCache> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<(Stream Content, string ContentType)?> GetAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured()) return null;

        var container = GetContainerClient();
        foreach (var (ext, contentType) in _formats)
        {
            var blobName = GetBlobName(cacheKey, ext);
            var blob = container.GetBlobClient(blobName);
            try
            {
                var response = await blob.DownloadStreamingAsync(cancellationToken: cancellationToken);
                return (response.Value.Content, contentType);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // Not found — try next format.
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error reading cached blob '{BlobName}'.", blobName);
                return null;
            }
        }

        return null;
    }

    public async Task SetAsync(string cacheKey, Stream image, string contentType, TimeSpan maxAge, CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured()) return;

        var ext = ContentTypeToExtension(contentType);
        var blobName = GetBlobName(cacheKey, ext);
        var container = GetContainerClient();
        var blob = container.GetBlobClient(blobName);

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
        };

        try
        {
            await blob.UploadAsync(image, uploadOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error writing cached blob '{BlobName}'.", blobName);
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured()) return;

        try
        {
            var container = GetContainerClient();
            var prefix = string.IsNullOrEmpty(_options.BasePath) ? null : _options.BasePath.TrimEnd('/') + '/';
            await foreach (var blob in container.GetBlobsAsync(BlobTraits.None, BlobStates.None, prefix, cancellationToken))
            {
                await container.DeleteBlobIfExistsAsync(blob.Name, cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error clearing Azure blob resized image cache.");
        }
    }

    public async Task ClearStaleAsync(TimeSpan maxAge, CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured()) return;

        var cutoff = DateTimeOffset.UtcNow - maxAge;
        try
        {
            var container = GetContainerClient();
            var prefix = string.IsNullOrEmpty(_options.BasePath) ? null : _options.BasePath.TrimEnd('/') + '/';
            await foreach (var blob in container.GetBlobsAsync(traits: BlobTraits.None, states: BlobStates.None, prefix: prefix, cancellationToken: cancellationToken))
            {
                if (blob.Properties.LastModified.HasValue && blob.Properties.LastModified.Value < cutoff)
                {
                    await container.DeleteBlobIfExistsAsync(blob.Name, cancellationToken: cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error clearing stale Azure blob resized image cache entries.");
        }
    }

    private BlobContainerClient GetContainerClient()
        => new(_options.ConnectionString, _options.ContainerName);

    private string GetBlobName(string cacheKey, string extension)
    {
        var prefix = string.IsNullOrEmpty(_options.BasePath) ? string.Empty : _options.BasePath.TrimEnd('/') + '/';
        var subDir = cacheKey.Length >= 2 ? cacheKey[..2] : "xx";
        return $"{prefix}{subDir}/{cacheKey}{extension}";
    }

    private static string ContentTypeToExtension(string contentType) => contentType switch
    {
        "image/png"  => ".png",
        "image/webp" => ".webp",
        "image/gif"  => ".gif",
        "image/bmp"  => ".bmp",
        _            => ".jpg",
    };
}
