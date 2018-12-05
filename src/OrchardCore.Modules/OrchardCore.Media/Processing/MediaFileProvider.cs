using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Helpers;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Resolvers;

namespace OrchardCore.Media.Processing
{
    public class MediaFileProvider : IImageProvider
    {
        private readonly IMediaFileStore _mediaStore;
        private readonly FormatHelper _formatHelper;

        public MediaFileProvider(IMediaFileStore mediaStore, IOptions<ImageSharpMiddlewareOptions> options)
        {
            _mediaStore = mediaStore;
            _formatHelper = new FormatHelper(options.Value.Configuration);
        }

        /// <inheritdoc/>
        public Func<HttpContext, bool> Match { get; set; } = _ => true;

        /// <inheritdoc/>
        public IDictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();

        /// <inheritdoc/>
        public bool IsValidRequest(HttpContext context) => _formatHelper.GetExtension(context.Request.GetDisplayUrl()) != null;

        /// <inheritdoc/>
        public IImageResolver Get(HttpContext context)
        {
            // Path has already been correctly parsed before here.

            var filePath = _mediaStore.MapPublicUrlToPath(context.Request.PathBase + context.Request.Path.Value);

            return new MediaFileResolver(_mediaStore, filePath);
        }
    }
}
