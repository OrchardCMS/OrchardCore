using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using OrchardCore.Infrastructure.Html;
using OrchardCore.ResourceManagement;
using Shortcodes;

namespace OrchardCore.Media.Shortcodes
{
    public class AssetUrlShortcodeProvider : IShortcodeProvider
    {
        private static readonly ValueTask<string> Null = new ValueTask<string>((string)null);
        private static readonly ValueTask<string> AssetUrlShortcode = new ValueTask<string>("[asset_url]");
        private const string Identifier = "asset_url";

        private readonly IMediaFileStore _mediaFileStore;
        private readonly IHtmlSanitizerService _htmlSanitizerService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ResourceManagementOptions _options;

        public AssetUrlShortcodeProvider(
            IMediaFileStore mediaFileStore,
            IHtmlSanitizerService htmlSanitizerService,
            IHttpContextAccessor httpContextAccessor,
            IOptions<ResourceManagementOptions> options)
        {
            _mediaFileStore = mediaFileStore;
            _htmlSanitizerService = htmlSanitizerService;
            _httpContextAccessor = httpContextAccessor;
            _options = options.Value;
        }

        public ValueTask<string> EvaluateAsync(string identifier, Arguments arguments, string content, Context context)
        {
            if (!String.Equals(identifier, Identifier, StringComparison.OrdinalIgnoreCase))
            {
                return Null;
            }

            // Handle self closing shortcodes.
            if (String.IsNullOrEmpty(content))
            {
                content = arguments.NamedOrDefault("src");
                if (String.IsNullOrEmpty(content))
                {
                    return AssetUrlShortcode;
                }
            }

            if (!content.StartsWith("//", StringComparison.Ordinal) && !content.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                // Serve static files from virtual path.
                if (content.StartsWith("~/", StringComparison.Ordinal))
                {
                    content = _httpContextAccessor.HttpContext.Request.PathBase.Add(content[1..]).Value;
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

            if (arguments.Any())
            {
                var queryStringParams = new Dictionary<string, string>();

                var width = arguments.Named("width");
                var height = arguments.Named("height");
                var mode = arguments.Named("mode");
                var quality = arguments.Named("quality");
                var format = arguments.Named("format");

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

                content = QueryHelpers.AddQueryString(content, queryStringParams);
            }

            // To sanitize the content, which may be later wrapped in a tag by the writer, we wrap it in an img tag, sanitize it, and remove the img tag.
            content = "<img src=\"" + content + "\">";
            content = _htmlSanitizerService.Sanitize(content);
            content = content[10..^2];

            return new ValueTask<string>(content);
        }
    }
}
