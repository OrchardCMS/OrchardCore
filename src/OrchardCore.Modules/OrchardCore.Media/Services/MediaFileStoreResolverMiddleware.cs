using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Routing;

namespace OrchardCore.Media.Services
{
    /// <summary>
    /// Provides local caching for remote file store files.
    /// </summary>
    public class MediaFileStoreResolverMiddleware
    {
        private static readonly ConcurrentDictionary<string, Lazy<Task>> Workers = new ConcurrentDictionary<string, Lazy<Task>>();

        private readonly RequestDelegate _next;
        private readonly ILogger<MediaFileStoreResolverMiddleware> _logger;
        private readonly IMediaFileStoreCache _mediaFileStoreCache;
        private readonly IMediaFileStore _mediaFileStore;

        private readonly PathString _assetsRequestPath;
        private readonly HashSet<string> _allowedFileExtensions;

        public MediaFileStoreResolverMiddleware(
            RequestDelegate next,
            ILogger<MediaFileStoreResolverMiddleware> logger,
            IMediaFileStoreCache mediaFileStoreCache,
            IMediaFileStore mediaFileStore,
            IOptions<MediaOptions> mediaOptions
            )
        {
            _next = next;
            _logger = logger;
            _mediaFileStoreCache = mediaFileStoreCache;
            _mediaFileStore = mediaFileStore;

            _assetsRequestPath = mediaOptions.Value.AssetsRequestPath;
            _allowedFileExtensions = mediaOptions.Value.AllowedFileExtensions;
        }

        /// <summary>
        /// Processes a request to determine if it matches a known file from the media file store, and cache it locally.
        /// </summary>
        /// <param name="context"></param>
        public async Task Invoke(HttpContext context)
        {
            //TODO for 3.0 and endpoint routing, validate it is not an endpoint

            // Support only Head requests or Get Requests.
            if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
            {
                await _next(context);
                return;
            }

            var validatePath = context.Request.Path.StartsWithNormalizedSegments(_assetsRequestPath, StringComparison.OrdinalIgnoreCase, out var subPath);
            if (!validatePath)
            {
                _logger.LogDebug("Request path {Path} does not match the assets request path {RequestPath}", subPath, _assetsRequestPath);
                await _next(context);
                return;
            }

            // subpath.Value returns an unescaped path value, subPath returns an escaped path value.
            var subPathValue = subPath.Value;

            // This will not cache the file if the file extension is not supported.
            var fileExtension = GetExtension(subPathValue);
            if (!_allowedFileExtensions.Contains(fileExtension))
            {
                _logger.LogDebug("File extension not supported for request path {Path}", subPathValue);
                await _next(context);
                return;
            }

            var isFileCached = await _mediaFileStoreCache.IsCachedAsync(subPathValue);
            if (isFileCached)
            {
                // When multiple requests occur for the same file the download 
                // may already be in progress so we wait for it to complete.
                if (Workers.TryGetValue(subPathValue, out var writeTask))
                {
                    await writeTask.Value;
                }

                await _next(context);
                return;
            }

            // When multiple requests occur for the same file we use a Lazy<Task>
            // to initialize the file store request once.
            await Workers.GetOrAdd(subPathValue, x => new Lazy<Task>(async () =>
            {
                try
                {
                    var fileStoreEntry = await _mediaFileStore.GetFileInfoAsync(subPathValue);

                    if (fileStoreEntry != null)
                    {
                        using (var stream = await _mediaFileStore.GetFileStreamAsync(fileStoreEntry))
                        {
                            await _mediaFileStoreCache.SetCacheAsync(stream, fileStoreEntry, context.RequestAborted);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the error, and pass to pipeline to handle as 404.
                    // Multiple requests at the same time will all recieve the same 404
                    // as we use LazyThreadSafetyMode.ExecutionAndPublication.
                    _logger.LogError(ex, "Error retrieving file from media file store for request path {Path}", subPathValue);
                }
                finally
                {
                    Workers.TryRemove(subPathValue, out var writeTask);
                }
            }, LazyThreadSafetyMode.ExecutionAndPublication)).Value;

            // Always call next, this middleware always passes.
            await _next(context);
            return;
        }

        private static string GetExtension(string path)
        {
            // Don't use Path.GetExtension as that may throw an exception if there are
            // invalid characters in the path. Invalid characters should be handled
            // by the FileProviders

            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var index = path.LastIndexOf('.');
            if (index < 0)
            {
                return null;
            }

            return path.Substring(index);
        }
    }
}
