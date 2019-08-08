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
        private const int StreamCopyBufferSize = 81920;

        private readonly IMediaFileStore _mediaFileStore;
        private readonly IMediaImageCache _mediaImageCache;
        private readonly PathString _subPath;
        private readonly string _fileExtension;

        private IFileStoreEntry _fileStoreEntry;
        private ImageMetaData _imageMetadata;

        public MediaFileStoreContext(
            HttpContext context,
            ILogger logger,
            IMediaFileStore mediaFileStore,
            IMediaImageCache mediaImageCache,
            int maxBrowserCacheDays,
            string contentType,
            string cacheKey,
            PathString subPath,
            string fileExtension
            ) : base
            (
                context,
                logger,
                maxBrowserCacheDays,
                contentType,
                cacheKey
            )
        {
            _mediaFileStore = mediaFileStore;
            _mediaImageCache = mediaImageCache;
            _subPath = subPath;
            _fileExtension = fileExtension;
        }

        public async Task<bool> LookupFileStoreInfo()
        {
            _fileStoreEntry = await _mediaFileStore.GetFileInfoAsync(_subPath.Value);
            if (_fileStoreEntry != null)
            {
                _length = _fileStoreEntry.Length;

                // Generate ImageSharp metadata now, so etag value will match when cached.
                _imageMetadata = new ImageMetaData(DateTime.UtcNow, _contentType, _maxBrowserCacheDays);
                DateTimeOffset last = _imageMetadata.LastWriteTimeUtc;

                // Truncate to the second.
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
                using (var readStream = await _mediaFileStore.GetFileStreamAsync(_fileStoreEntry))
                {
                    // Serve to the client first.

                    // Use Default StreamCopyBufferSize to stay below the large object heap threshold.
                    await StreamCopyOperation.CopyToAsync(readStream, _response.Body, _length, StreamCopyBufferSize, _context.RequestAborted);

                    // Reset the stream and write to cache, this does not cause a second request to blob storage.
                    if (readStream.CanSeek)
                    {
                        readStream.Position = 0;
                        await _mediaImageCache.TrySetAsync(_cacheKey, _fileExtension, readStream, _imageMetadata);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                //TODO log correctly
                Logger.LogInformation(ex.Message);
                // Don't throw this exception, it's most likely caused by the client disconnecting.
                // However, if it was cancelled for any other reason we need to prevent empty responses.
                _context.Abort();
            }
        }
    }
}
