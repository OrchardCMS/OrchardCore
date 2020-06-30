using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

                // TODO add imagesharp args for width height resizemode
                // apply ~/ virtual path
                // skip public url when http https or //

                // possibly use tagbuilder

                var publicUrl = mediaFileStore.MapPathToPublicUrl(content);

                var tag = "<img src=\"" + publicUrl + "\">";
                tag = htmlSanitizerService.Sanitize(tag);

                return new ValueTask<string>(tag);
            };
        }
    }
}
