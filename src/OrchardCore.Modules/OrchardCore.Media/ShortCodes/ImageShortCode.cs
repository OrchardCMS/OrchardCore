using System.Threading.Tasks;
using OrchardCore.Infrastructure.Html;
using OrchardCore.ShortCodes;

namespace OrchardCore.Media.ShortCodes
{
    public class ImageShortCode : ShortCode
    {
        private readonly IHtmlSanitizerService _htmlSanitizerService;
        private readonly IMediaFileStore _mediaFileStore;

        public ImageShortCode(IMediaFileStore mediaFileStore, IHtmlSanitizerService htmlSanitizerService)
        {
            _mediaFileStore = mediaFileStore;
            _htmlSanitizerService = htmlSanitizerService;
        }

        public override string Name => "media";

        public async override Task ProcessAsync(ShortCodeContext context, ShortCodeOutput output)
        {
            var url = await output.GetChildContentAsync();
            var tag = "<img src=\"" + url.GetContent() + "\"/>";
            output.Content.AppendHtml(tag);
        }
    }
}
