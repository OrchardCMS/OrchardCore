using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using OrchardCore.FileStorage;

namespace OrchardCore.Media.Azure.Middleware
{
    /// <summary>
    /// File store context to serve async media from blob storage,
    /// inspired by aspnetcore static file middleware,
    /// and adapted under the Apache License.
    /// </summary>
    public class MediaFileStoreContext : BaseFileContext
    {
        private const int StreamCopyBufferSize = 64 * 1024;

        private readonly IMediaFileStore _fileProvider;
        private readonly IShellImageCache _shellImageCache;
        private readonly string _cacheFilePath;

        private IFileStoreEntry _fileStoreEntry;

        public MediaFileStoreContext(
            HttpContext context,
            ILogger logger,
            IMediaFileStore fileProvider,
            PathString subPath,
            int maxBrowserCacheDays,
            string contentType,
            IShellImageCache shellImageCache,
            string cacheFilePath
            ) : base(
                context,
                logger,
                subPath,
                maxBrowserCacheDays,
                contentType
                )
        {
            _fileProvider = fileProvider;
            _shellImageCache = shellImageCache;
            _cacheFilePath = cacheFilePath;
        }

        public async Task<bool> LookupFileStoreInfo()
        {
            _fileStoreEntry = await _fileProvider.GetFileInfoAsync(_subPath.Value);
            if (_fileStoreEntry != null)
            {
                _length = _fileStoreEntry.Length;

                DateTimeOffset last = _fileStoreEntry.LastModifiedUtc;
                // Truncate to the second.
                _lastModified = new DateTimeOffset(last.Year, last.Month, last.Day, last.Hour, last.Minute, last.Second, last.Offset).ToUniversalTime();

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
                        await _shellImageCache.TrySetAsync(_cacheFilePath, readStream);
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
