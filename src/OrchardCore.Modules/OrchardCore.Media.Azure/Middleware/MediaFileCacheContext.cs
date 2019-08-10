using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Helpers;

namespace OrchardCore.Media.Azure.Middleware
{
    /// <summary>
    /// Media file cache context to serve media assets from file system cache.
    /// </summary>
    public class MediaFileCacheContext : BaseFileContext
    {
        private const int StreamCopyBufferSize = 64 * 1024;

        private readonly IMediaImageCache _mediaImageCache;
        private readonly FormatUtilities _formatUtilities;

        private IMediaCacheFileResolver _mediaCacheFileResolver;
        private ImageMetaData _imageMetaData;

        public MediaFileCacheContext(
            HttpContext context,
            ILogger logger,
            IMediaImageCache mediaImageCache,
            FormatUtilities formatUtilities,
            int maxBrowserCacheDays,
            string contentType,
            string cacheKey,
            string fileExtension,
            PathString subPath
            ) : base(
                context,
                logger,
                maxBrowserCacheDays,
                contentType,
                cacheKey,
                fileExtension,
                subPath
                )
        {
            _mediaImageCache = mediaImageCache;
            _formatUtilities = formatUtilities;
        }

        public async Task<bool> LookupFileInfo()
        {
            // TODO Consider how to provide a graceful cache period on this, 
            // so we do not serve a file from cache, if it has been deleted from Blob storage.
            // One idea: Treat this cache like any other Cdn cache. Cache bust it.
            // We could pull the v=hash from the query string here, and check the
            // IFileStoreVersionProvider to see if the version hash is the same
            // as the file store version hash.
            // If it is not, then remove the file from the disc cache.
            // The IFileStoreVersionProvider then becomes responsible for
            // gracefully expiring it's own memory cache of version hashes.

            // When cached by ImageSharp .jpeg will be cached as .jpg, so resolve.
            var fileExtension = String.Empty;
            try
            {
                fileExtension = '.' + _formatUtilities.GetExtensionFromContentType(_contentType);
            }
            catch (KeyNotFoundException)
            {
                fileExtension = _fileExtension;
            }

            // Resolve file through cache so we have correct value for the etag.
            _mediaCacheFileResolver = await _mediaImageCache.GetMediaCacheFileAsync(_cacheKey, fileExtension);

            if (_mediaCacheFileResolver == null)
                return false;

            _imageMetaData = await _mediaCacheFileResolver.GetMetaDataAsync();
            _length = _mediaCacheFileResolver.Length;

            DateTimeOffset last = _imageMetaData.LastWriteTimeUtc;
            // Truncate to the second.
            _lastModified = new DateTimeOffset(last.Year, last.Month, last.Day, last.Hour, last.Minute, last.Second, last.Offset).ToUniversalTime();

            long etagHash = _lastModified.ToFileTime() ^ _length;

            _etag = new EntityTagHeaderValue('\"' + Convert.ToString(etagHash, 16) + '\"');

            return true;
        }

        public async override Task SendAsync()
        {
            ApplyResponseHeaders(StatusCodes.Status200OK);
            var physicalPath = _mediaCacheFileResolver.PhysicalPath;
            var sendFile = _context.Features.Get<IHttpSendFileFeature>();
            if (sendFile != null && !string.IsNullOrEmpty(physicalPath))
            {
                // We don't need to directly cancel this, if the client disconnects it will fail silently.
                await sendFile.SendFileAsync(physicalPath, 0, _length, CancellationToken.None);
                return;
            }

            try
            {
                using (var readStream = await _mediaCacheFileResolver.OpenReadAsync())
                {
                    // Larger StreamCopyBufferSize is required because in case of FileStream readStream isn't going to be buffering
                    await StreamCopyOperation.CopyToAsync(readStream, _response.Body, _length, StreamCopyBufferSize, _context.RequestAborted);
                }
            }
            catch (OperationCanceledException ex)
            {
                Logger.LogInformation(ex, "Cache transmission operation cancelled for request path {0}", _subPath);
                // Don't throw this exception, it's most likely caused by the client disconnecting.
                // However, if it was cancelled for any other reason we need to prevent empty responses.
                _context.Abort();
            }
        }
    }
}
