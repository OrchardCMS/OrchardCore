using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Routing;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Resolvers;

namespace OrchardCore.Media.Processing
{
    public class MediaResizingFileProvider : IImageProvider
    {
        private readonly IMediaFileProvider _mediaFileProvider;
        private readonly FormatUtilities _formatUtilities;
        private readonly int[] _supportedSizes;
        private readonly PathString _assetsRequestPath;

        /// <summary>
        /// A match function used by the resolver to identify itself as the correct resolver to use.
        /// </summary>
        private Func<HttpContext, bool> _match;

        public MediaResizingFileProvider(
            IMediaFileProvider mediaFileProvider,
            IOptions<ImageSharpMiddlewareOptions> imageSharpOptions,
            IOptions<MediaOptions> mediaOptions
            )
        {
            _mediaFileProvider = mediaFileProvider;
            _formatUtilities = new FormatUtilities(imageSharpOptions.Value.Configuration);
            _supportedSizes = mediaOptions.Value.SupportedSizes;
            _assetsRequestPath = mediaOptions.Value.AssetsRequestPath;
        }

        /// <inheritdoc/>
        public Func<HttpContext, bool> Match
        {
            get => _match ?? IsMatch;
            set => _match = value;
        }

        public ProcessingBehavior ProcessingBehavior => ProcessingBehavior.CommandOnly;

        /// <inheritdoc/>
        public bool IsValidRequest(HttpContext context)
        {
            if (_formatUtilities.GetExtensionFromUri(context.Request.GetDisplayUrl()) == null)
            {
                return false;
            }

            if (context.Request.Query.TryGetValue(ResizeWebProcessor.Width, out var widthString))
            {
                var width = CommandParser.Instance.ParseValue<int>(widthString);

                if (Array.BinarySearch<int>(_supportedSizes, width) < 0)
                {
                    return false;
                }
            }

            if (context.Request.Query.TryGetValue(ResizeWebProcessor.Height, out var heightString))
            {
                var height = CommandParser.Instance.ParseValue<int>(heightString);

                if (Array.BinarySearch<int>(_supportedSizes, height) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public Task<IImageResolver> GetAsync(HttpContext context)
        {
            // Remove assets request path.
            var path = context.Request.Path.Value.Substring(_assetsRequestPath.Value.Length);

            var fileInfo = _mediaFileProvider.GetFileInfo(path);

            // Check to see if the file exists.
            if (!fileInfo.Exists)
            {
                return Task.FromResult<IImageResolver>(null);
            }

            // We don't care about the content type nor cache control max age here.
            var metadata = new ImageMetadata(fileInfo.LastModified.UtcDateTime);
            return Task.FromResult<IImageResolver>(new PhysicalFileSystemResolver(fileInfo, metadata));
        }

        private bool IsMatch(HttpContext context)
        {
            if (!context.Request.Path.StartsWithNormalizedSegments(_assetsRequestPath, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }
    }
}
