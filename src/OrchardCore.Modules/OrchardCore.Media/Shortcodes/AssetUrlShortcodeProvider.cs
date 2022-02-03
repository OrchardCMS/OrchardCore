using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;
using Shortcodes;

namespace OrchardCore.Media.Shortcodes
{
    public class AssetUrlShortcodeProvider : IShortcodeProvider
    {
        private readonly IMediaFileStore _mediaFileStore;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ResourceManagementOptions _options;

        public AssetUrlShortcodeProvider(
            IMediaFileStore mediaFileStore,
            IHttpContextAccessor httpContextAccessor,
            IOptions<ResourceManagementOptions> options)
        {
            _mediaFileStore = mediaFileStore;
            _httpContextAccessor = httpContextAccessor;
            _options = options.Value;
        }

        public ValueTask<string> EvaluateAsync(string identifier, Arguments arguments, string content, Context context)
        {
            if (!String.Equals(identifier, "asset_url", StringComparison.OrdinalIgnoreCase))
            {
                return new ValueTask<string>((string)null);
            }

            // Handle self closing shortcodes.
            if (String.IsNullOrEmpty(content))
            {
                content = arguments.NamedOrDefault("src");
                if (String.IsNullOrEmpty(content))
                {
                    return new ValueTask<string>("[asset_url]");
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

            // This does not produce a tag, so sanitization is performed by the consumer (html body or markdown).

            return new ValueTask<string>(content);
        }
    }
}
