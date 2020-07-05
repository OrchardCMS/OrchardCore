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
    public class ImageShortcodeProvider : NamedShortcodeProvider
    {
        public ImageShortcodeProvider(
            IMediaFileStore mediaFileStore,
            IHtmlSanitizerService htmlSanitizerService,
            IHttpContextAccessor httpContextAccessor,
            IOptions<ResourceManagementOptions> options)
            : base(
                new Dictionary<string, ShortcodeDelegate>
                {
                    ["image"] = EvaluateImageAsync(mediaFileStore, htmlSanitizerService, httpContextAccessor, options.Value),
                    ["media"] = EvaluateImageAsync(mediaFileStore, htmlSanitizerService, httpContextAccessor, options.Value)
                }
        )
        {}

        private static ShortcodeDelegate EvaluateImageAsync(
            IMediaFileStore mediaFileStore,
            IHtmlSanitizerService htmlSanitizerService,
            IHttpContextAccessor httpContextAccessor,
            ResourceManagementOptions options)
        {
            return (args, content) =>
            {
                // Handle edge case of self closing shortcodes.
                if (String.IsNullOrEmpty(content))
                {
                    content = args.NamedOrDefault("src");
                    if (String.IsNullOrEmpty(content))
                    {
                        // Do not handle the deprecated media shortcode in this edge case.
                        return new ValueTask<string>("[image]");
                    }
                }

                if (!content.StartsWith("//", StringComparison.Ordinal) && !content.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    // Serve static files from virtual path.
                    if (content.StartsWith("~/", StringComparison.Ordinal))
                    {
                        content = httpContextAccessor.HttpContext.Request.PathBase.Add(content.Substring(1)).Value;
                        if (!String.IsNullOrEmpty(options.CdnBaseUrl))
                        {
                            content = options.CdnBaseUrl + content;
                        }
                    }
                    else
                    {
                        content = mediaFileStore.MapPathToPublicUrl(content);
                    }
                }

                if (args.Any())
                {
                    var queryStringParams = new Dictionary<string, string>();

                    var width = args.Named("width");
                    var height = args.Named("height");
                    var mode = args.Named("mode");

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

                    content = QueryHelpers.AddQueryString(content, queryStringParams);
                }

                var tag = "<img src=\"" + content + "\">";
                tag = htmlSanitizerService.Sanitize(tag);

                return new ValueTask<string>(tag);
            };
        }
    }
}
