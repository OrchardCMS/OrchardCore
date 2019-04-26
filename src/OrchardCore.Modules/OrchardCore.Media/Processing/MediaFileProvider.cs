using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Helpers;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Resolvers;

namespace OrchardCore.Media.Processing
{
    public class MediaFileProvider : IImageProvider
    {
        private readonly IMediaFileStore _mediaStore;
        private readonly FormatUtilities _formatUtilities;

        public MediaFileProvider(IMediaFileStore mediaStore, IOptions<ImageSharpMiddlewareOptions> options)
        {
            _mediaStore = mediaStore;
            _formatUtilities = new FormatUtilities(options.Value.Configuration);
        }

        /// <inheritdoc/>
        public Func<HttpContext, bool> Match { get; set; } = _ => true;

        /// <inheritdoc/>
        public IDictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();

        /// <inheritdoc/>
        public bool IsValidRequest(HttpContext context) => _formatUtilities.GetExtensionFromUri(context.Request.GetDisplayUrl()) != null;

        /// <inheritdoc/>
        public async Task<IImageResolver> GetAsync(HttpContext context)
        {
            // Path has already been correctly parsed before here.
            var filePath = _mediaStore.MapPublicUrlToPath(context.Request.PathBase + context.Request.Path.Value);

            // Check to see if the file exists.
            var file = await _mediaStore.GetFileInfoAsync(filePath);
            if (file == null)
            {
                return null;
            }
            var metadata = new ImageMetaData(file.LastModifiedUtc);
            return new MediaFileResolver(_mediaStore, filePath, metadata);
        }
    }
}
