using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using OrchardCore.FileStorage;
using SixLabors.ImageSharp.Web;

namespace OrchardCore.Media.Azure.Middleware
{
    // Adapted under the apache 2.0 license from AspNetCore.StaticFileMiddleware, and ImageSharp.Web.

    /// <summary>
    /// File store context to serve async media from Azure Blob Storage.
    /// </summary>
    public class MediaFileStoreContext : BaseFileContext
    {
        private const int StreamCopyBufferSize = 64 * 1024;

        private readonly IMediaFileStore _fileProvider;
        private readonly IMediaImageCache _mediaImageCache;
        private readonly PathString _subPath;
        private readonly string _cacheKey;
        private readonly string _extension;

        private IFileStoreEntry _fileStoreEntry;

        public MediaFileStoreContext(
            HttpContext context,
            ILogger logger,
            IMediaFileStore fileProvider,
            PathString subPath,
            int maxBrowserCacheDays,
            string contentType,
            IMediaImageCache mediaImageCache,
            string cacheKey,
            string extension
            ) : base(
                context,
                logger,
                maxBrowserCacheDays,
                contentType
                )
        {
            _fileProvider = fileProvider;
            _mediaImageCache = mediaImageCache;
            _cacheKey = cacheKey;
            _extension = extension;
            _subPath = subPath;
        }

        public async Task<bool> LookupFileStoreInfo()
        {
            _fileStoreEntry = await _fileProvider.GetFileInfoAsync(_subPath.Value);
            if (_fileStoreEntry != null)
            {
                _length = _fileStoreEntry.Length;

                // We need to use the same technique as ImageSharp to produce a consistent etag.

                DateTimeOffset last = DateTime.UtcNow;
                // Truncate to the minute, to allow the cached entry to be the same.
                _lastModified = new DateTimeOffset(last.Year, last.Month, last.Day, last.Hour, last.Minute, 0, last.Offset).ToUniversalTime();

                long etagHash = _lastModified.ToFileTime() ^ _length;
                _etag = new EntityTagHeaderValue('\"' + Convert.ToString(etagHash, 16) + '\"');
                return true;
            }
            return false;
        }

        public async override Task SendAsync()
        {
            ApplyResponseHeaders(StatusCodes.Status200OK);

            try
            {
                using (var readStream = await _fileProvider.GetFileStreamAsync(_fileStoreEntry))
                {
                    // Serve to the client first
                    // TODO Come back to this as Azure blob should be buffering. Need to have a look at MultiBufferMemoryStream
                    // Larger StreamCopyBufferSize is required because in case of FileStream readStream isn't going to be buffering
                    await StreamCopyOperation.CopyToAsync(readStream, _response.Body, _length, StreamCopyBufferSize, _context.RequestAborted);

                    // Reset the stream and write to cache, this does not cause a second request to blob storage
                    if (readStream.CanSeek)
                    {
                        readStream.Position = 0;

                        // Use same metadata as ImageSharp so cache resolves etag correctly.
                        var cachedImageMetadata = new ImageMetaData(DateTime.UtcNow, _contentType, _maxBrowserCacheDays);
                        await _mediaImageCache.TrySetAsync(_cacheKey, _extension, readStream, cachedImageMetadata);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                //TODO log correctly
                _logger.LogInformation(ex.Message);
                // Don't throw this exception, it's most likely caused by the client disconnecting.
                // However, if it was cancelled for any other reason we need to prevent empty responses.
                _context.Abort();
            }
        }
    }
}
