using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Shortcodes;
using OrchardCore.Infrastructure.Html;

namespace OrchardCore.Media.Shortcodes
{
    public class ImageShortcodeProvider : NamedShortcodeProvider
    {
        private readonly IHtmlSanitizerService _htmlSanitizerService;
        private readonly IMediaFileStore _mediaFileStore;

        public ImageShortcodeProvider(IMediaFileStore mediaFileStore, IHtmlSanitizerService htmlSanitizerService)
            : base(
            new Dictionary<string, ShortcodeDelegate>
            {
                ["image"] = EvaluateImageAsync(mediaFileStore, htmlSanitizerService),
                ["media"] = EvaluateImageAsync(mediaFileStore, htmlSanitizerService)
            }
        )
        {
            _mediaFileStore = mediaFileStore;
            _htmlSanitizerService = htmlSanitizerService;
        }

        private static ShortcodeDelegate EvaluateImageAsync(IMediaFileStore mediaFileStore, IHtmlSanitizerService htmlSanitizerService)
        {
            return (args, content) =>
            {
                // Handle edge case of self closing shortcodes.
                if (string.IsNullOrEmpty(content))
                {
                    content = args.NamedOrDefault("src");
                    if (string.IsNullOrEmpty(content))
                    {
                        // Do not handle the deprecated media shortcode in this edge case.
                        return new ValueTask<string>("[image]");
                    }
                }

                // TODO handle virtual path, i.e. ~/
                if (!content.StartsWith("//", StringComparison.Ordinal) && !content.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    content = mediaFileStore.MapPathToPublicUrl(content);
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
