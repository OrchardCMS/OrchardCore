using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ImageSharp;
using ImageSharp.Formats;
using ImageSharp.Memory;
using ImageSharp.Web.Helpers;
using ImageSharp.Web.Middleware;
using ImageSharp.Web.Resolvers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Orchard.Media.Processing
{
    /// <summary>
    /// Returns images stored in the local physical file system.
    /// </summary>
    public class MediaFileSystemResolver : IImageResolver
    {
        private readonly IMediaFileStore _mediaStore;

        /// <summary>
        /// The middleware configuration options.
        /// </summary>
        private readonly ImageSharpMiddlewareOptions options;

        public MediaFileSystemResolver(IMediaFileStore mediaStore, IOptions<ImageSharpMiddlewareOptions> options)
        {
            _mediaStore = mediaStore;
            this.options = options.Value;
        }

        /// <inheritdoc/>
        public Func<HttpContext, bool> Match { get; set; } = _ => true;

        /// <inheritdoc/>
        public IDictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();

        /// <inheritdoc/>
        public Task<bool> IsValidRequestAsync(HttpContext context, ILogger logger)
        {
            return Task.FromResult(FormatHelpers.GetExtension(this.options.Configuration, context.Request.GetDisplayUrl()) != null);
        }

        /// <inheritdoc/>
        public async Task<byte[]> ResolveImageAsync(HttpContext context, ILogger logger)
        {
            // Path has already been correctly parsed before here.

            var file = await _mediaStore.MapFileAsync(context.Request.Path);

            byte[] buffer;

            // Check to see if the file exists.
            if (file == null)
            {
                return null;
            }

            using (Stream stream = file.CreateReadStream())
            {
                // Buffer is returned to the pool in the middleware
                buffer = BufferDataPool.Rent((int)stream.Length);
                await stream.ReadAsync(buffer, 0, (int)stream.Length);
            }

            return buffer;
        }
    }
}
