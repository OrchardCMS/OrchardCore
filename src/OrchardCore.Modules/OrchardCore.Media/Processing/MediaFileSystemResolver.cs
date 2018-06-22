using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Helpers;
using SixLabors.ImageSharp.Web.Memory;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Resolvers;

namespace OrchardCore.Media.Processing
{
    /// <summary>
    /// Returns images stored in the local physical file system.
    /// </summary>
    public class MediaFileSystemResolver : IImageResolver
    {
        private readonly IBufferManager bufferManager;
        private readonly IMediaFileStore _mediaStore;

        /// <summary>
        /// The middleware configuration options.
        /// </summary>
        private readonly ImageSharpMiddlewareOptions options;

        public MediaFileSystemResolver(IMediaFileStore mediaStore, IBufferManager bufferManager, IOptions<ImageSharpMiddlewareOptions> options)
        {
            _mediaStore = mediaStore;
            this.options = options.Value;
            this.bufferManager = bufferManager;
        }

        /// <inheritdoc/>
        public Func<HttpContext, bool> Match { get; set; } = _ => true;

        /// <inheritdoc/>
        public IDictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();

        /// <inheritdoc/>
        public Task<bool> IsValidRequestAsync(HttpContext context, ILogger logger)
        {
            return Task.FromResult(FormatHelpers.GetExtension(this.options.Configuration, context.Request.Path) != null);
        }

        /// <inheritdoc/>
        public async Task<IByteBuffer> ResolveImageAsync(HttpContext context, ILogger logger)
        {
            // Path has already been correctly parsed before here.

            var filePath = _mediaStore.MapPublicUrlToPath(context.Request.PathBase + context.Request.Path.Value);
            var file = await _mediaStore.GetFileInfoAsync(filePath);

            // Check to see if the file exists.
            if (file == null)
            {
                return null;
            }

            IByteBuffer buffer;

            using (var stream = await _mediaStore.GetFileStreamAsync(filePath))
            {
                // Buffer is returned to the pool in the middleware
                buffer = this.bufferManager.Allocate((int)stream.Length);
                await stream.ReadAsync(buffer.Array, 0, buffer.Length);
            }

            return buffer;
        }
    }
}
