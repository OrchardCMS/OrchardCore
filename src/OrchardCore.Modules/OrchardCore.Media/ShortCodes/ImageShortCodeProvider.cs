using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shortcodes;
using OrchardCore.DisplayManagement;
using OrchardCore.Infrastructure.Html;
using OrchardCore.ShortCodes;

namespace OrchardCore.Media.ShortCodes
{
    public class ImageShortCodeProvider : NamedShortcodeProvider
    {
        private readonly IHtmlSanitizerService _htmlSanitizerService;
        private readonly IMediaFileStore _mediaFileStore;

        public ImageShortCodeProvider(IMediaFileStore mediaFileStore, IHtmlSanitizerService htmlSanitizerService)
            : base(
            new Dictionary<string, ShortcodeDelegate>
            {
                ["image"] = EvaluateImageAsync(mediaFileStore, htmlSanitizerService)
                // ["image"] = new ShortcodeDelegate(EvaluateImageAsync),
                // ["media"] = new ShortcodeDelegate(EvaluateImageAsync)
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
                // var t = mediaFileStore.MapPathToPublicUrl("about-bg.jpg");

                return new ValueTask<string>(tag);
            };
        }

        // private static ValueTask<string> EvaluateImageAsync(Shortcodes.Arguments arguments, string content)
        // {
        //     return new ValueTask<string>("Hello world!");
        // }
    }

    //     private static ValueTask<string> EvaluateImageAsync(Shortcodes.Arguments arguments, string content)
    // {
    //     return new ValueTask<string>("Hello world!");
    // }
}
