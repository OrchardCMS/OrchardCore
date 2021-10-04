using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Media.Processing;
using OrchardCore.ResourceManagement;
using Shortcodes;

namespace OrchardCore.Media.Shortcodes
{
    public class ImageShortcodeProvider : IShortcodeProvider
    {
        private static readonly ValueTask<string> Null = new ValueTask<string>((string)null);
        private static readonly ValueTask<string> ImageShortcode = new ValueTask<string>("[image]");
        private static readonly HashSet<string> Shortcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image",
            "media" // [media] is a deprecated shortcode, and can be removed in a future release.
        };

        private readonly IMediaFileStore _mediaFileStore;
        private readonly IHtmlSanitizerService _htmlSanitizerService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ResourceManagementOptions _options;
        private readonly IOrchardHelper _orchardHelper;

        public ImageShortcodeProvider(
            IMediaFileStore mediaFileStore,
            IHtmlSanitizerService htmlSanitizerService,
            IHttpContextAccessor httpContextAccessor,
            IOptions<ResourceManagementOptions> options,
            IOrchardHelper orchardHelper)
        {
            _mediaFileStore = mediaFileStore;
            _htmlSanitizerService = htmlSanitizerService;
            _httpContextAccessor = httpContextAccessor;
            _options = options.Value;
            _orchardHelper = orchardHelper;
        }

        public ValueTask<string> EvaluateAsync(string identifier, Arguments arguments, string content, Context context)
        {
            if (!Shortcodes.Contains(identifier))
            {
                return Null;
            }

            // Handle self closing shortcodes.
            if (String.IsNullOrEmpty(content))
            {
                content = arguments.NamedOrDefault("src");
                if (String.IsNullOrEmpty(content))
                {
                    // Do not handle the deprecated media shortcode in this edge case.
                    return ImageShortcode;
                }
            }

            if (!content.StartsWith("//", StringComparison.Ordinal) && !content.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                // Serve static files from virtual path.
                if (content.StartsWith("~/", StringComparison.Ordinal))
                {
                    content = _httpContextAccessor.HttpContext.Request.PathBase.Add(content.Substring(1)).Value;
                    if (!String.IsNullOrEmpty(_options.CdnBaseUrl))
                    {
                        content = _options.CdnBaseUrl + content;
                    }
                }
                else
                {
                    content = _mediaFileStore.MapPathToPublicUrl(content);
                }
            }
            var className = string.Empty;
            var altText = string.Empty;
            if (arguments.Any())
            {
                int? width = int.TryParse(arguments.Named("width"), out var widthValue) ? widthValue : null;
                int? height = int.TryParse(arguments.Named("height"), out var heightValue) ? heightValue : null;
                var mode = Enum.TryParse<ResizeMode>(arguments.Named("mode"), true, out var modeValue) ? modeValue : ResizeMode.Undefined;
                int? quality = int.TryParse(arguments.Named("quality"), out var qualityValue) ? qualityValue : null;
                var format = Enum.TryParse<Format>(arguments.Named("format"), true, out var formatValue) ? formatValue : Format.Undefined;

                className = arguments.Named("class");
                altText = arguments.Named("alt");

                if (className != null)
                {
                    className = "class=\"" + className + "\" ";
                }

                if (altText != null)
                {
                    altText = "alt=\"" + altText + "\" ";
                }

                content = _orchardHelper.ImageResizeUrl(content, width, height, mode, quality, format);
            }

            content = "<img " + altText + className + "src=\"" + content + "\">";
            content = _htmlSanitizerService.Sanitize(content);

            return new ValueTask<string>(content);
        }
    }
}
