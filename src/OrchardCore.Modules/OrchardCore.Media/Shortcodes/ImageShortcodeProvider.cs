using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using OrchardCore.Infrastructure.Html;
using OrchardCore.ResourceManagement;
using Shortcodes;

namespace OrchardCore.Media.Shortcodes;

public class ImageShortcodeProvider : IShortcodeProvider
{
    private static ValueTask<string> Null => new((string)null);
    private static ValueTask<string> ImageShortcode => new("[image]");
    private static readonly HashSet<string> _shortcodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image",
        "media" // [media] is a deprecated shortcode, and can be removed in a future release.
    };

    private readonly IMediaFileStore _mediaFileStore;
    private readonly IHtmlSanitizerService _htmlSanitizerService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ResourceManagementOptions _options;
    private readonly IFileVersionProvider _fileVersionProvider;

    public ImageShortcodeProvider(
        IMediaFileStore mediaFileStore,
        IHtmlSanitizerService htmlSanitizerService,
        IHttpContextAccessor httpContextAccessor,
        IOptions<ResourceManagementOptions> options,
        IFileVersionProvider fileVersionProvider)
    {
        _mediaFileStore = mediaFileStore;
        _htmlSanitizerService = htmlSanitizerService;
        _httpContextAccessor = httpContextAccessor;
        _options = options.Value;
        _fileVersionProvider = fileVersionProvider;
    }

    public ValueTask<string> EvaluateAsync(string identifier, Arguments arguments, string content, Context context)
    {
        if (!_shortcodes.Contains(identifier))
        {
            return Null;
        }

        // Handle self closing shortcodes.
        if (string.IsNullOrEmpty(content))
        {
            content = arguments.NamedOrDefault("src");
            if (string.IsNullOrEmpty(content))
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
                content = _httpContextAccessor.HttpContext.Request.PathBase.Add(content[1..]).Value;
                if (!string.IsNullOrEmpty(_options.CdnBaseUrl))
                {
                    content = _options.CdnBaseUrl + content;
                }
            }
            else
            {
                content = content.RemoveQueryString(out var queryString);
                content = _mediaFileStore.MapPathToPublicUrl(content) + queryString;
            }
        }
        var className = string.Empty;
        var altText = string.Empty;
        if (arguments.Any())
        {
            var queryStringParams = new Dictionary<string, string>();

            var width = arguments.Named("width");
            var height = arguments.Named("height");
            var mode = arguments.Named("mode");
            var quality = arguments.Named("quality");
            var format = arguments.Named("format");
            var appendVersion = arguments.Named("append_version");
            className = arguments.Named("class");
            altText = arguments.Named("alt");

            if (width != null)
            {
                queryStringParams.Add("width", width);
            }

            if (height != null)
            {
                queryStringParams.Add("height", height);
            }

            if (mode != null)
            {
                queryStringParams.Add("rmode", mode);
            }

            if (quality != null)
            {
                queryStringParams.Add("quality", quality);
            }

            if (format != null)
            {
                queryStringParams.Add("format", format);
            }

            if (appendVersion?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                content = _fileVersionProvider.AddFileVersionToPath(_httpContextAccessor.HttpContext.Request.PathBase, content);
            }

            if (className != null)
            {
                className = "class=\"" + className + "\" ";
            }

            if (altText != null)
            {
                altText = "alt=\"" + altText + "\" ";
            }

            content = QueryHelpers.AddQueryString(content, queryStringParams);
        }

        content = "<img " + altText + className + "src=\"" + content + "\">";
        content = _htmlSanitizerService.Sanitize(content);

        return ValueTask.FromResult(content);
    }
}
