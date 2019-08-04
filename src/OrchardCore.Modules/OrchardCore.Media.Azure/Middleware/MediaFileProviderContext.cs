using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace OrchardCore.Media.Azure.Middleware
{
    /// <summary>
    /// File provider context to serve media assets from file system cache,,
    /// inspired by aspnetcore static file middleware,
    /// and adapted under the Apache License.
    /// </summary>
    public class MediaFileProviderContext : BaseFileContext
    {
        private const int StreamCopyBufferSize = 64 * 1024;

        private readonly IFileProvider _fileProvider;

        private IFileInfo _fileInfo;

        public MediaFileProviderContext(
            HttpContext context,
            ILogger logger,
            IFileProvider fileProvider,
            PathString cacheFilePath,
            int maxBrowserCacheDays,
            string contentType
            ) : base(
                context,
                logger,
                cacheFilePath,
                maxBrowserCacheDays,
                contentType
                )
        {
            _fileProvider = fileProvider;
            _fileInfo = null;
        }

        public bool LookupFileInfo()
        {
            _fileInfo = _fileProvider.GetFileInfo(_subPath.Value);
            if (_fileInfo.Exists)
            {
                _length = _fileInfo.Length;

                var last = _fileInfo.LastModified;
                // Truncate to the second.
                _lastModified = new DateTimeOffset(last.Year, last.Month, last.Day, last.Hour, last.Minute, last.Second, last.Offset).ToUniversalTime();

                long etagHash = _lastModified.ToFileTime() ^ _length;
                _etag = new EntityTagHeaderValue('\"' + Convert.ToString(etagHash, 16) + '\"');
            }
            return _fileInfo.Exists;
        }


        public async override Task SendAsync()
        {
            ApplyResponseHeaders(StatusCodes.Status200OK);
            var physicalPath = _fileInfo.PhysicalPath;
            var sendFile = _context.Features.Get<IHttpSendFileFeature>();
            if (sendFile != null && !string.IsNullOrEmpty(physicalPath))
            {
                // We don't need to directly cancel this, if the client disconnects it will fail silently.
                await sendFile.SendFileAsync(physicalPath, 0, _length, CancellationToken.None);
                return;
            }

            try
            {
                using (var readStream = _fileInfo.CreateReadStream())
                {
                    // Larger StreamCopyBufferSize is required because in case of FileStream readStream isn't going to be buffering
                    await StreamCopyOperation.CopyToAsync(readStream, _response.Body, _length, StreamCopyBufferSize, _context.RequestAborted);
                }
            }
            catch (OperationCanceledException)
            {
                //TODO log correctly
                //_logger.WriteCancelled(ex);
                // Don't throw this exception, it's most likely caused by the client disconnecting.
                // However, if it was cancelled for any other reason we need to prevent empty responses.
                _context.Abort();
            }
        }
    }
}
