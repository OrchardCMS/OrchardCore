using System.Threading.Tasks;
using OrchardCore.Infrastructure.Html;
using OrchardCore.ShortCodes;

namespace OrchardCore.Media.ShortCodes
{
    [ShortCodeTarget("image")]
    [ShortCodeTarget("media")]
    public class ImageShortCode : ShortCode
    {
        private readonly IHtmlSanitizerService _htmlSanitizerService;
        private readonly IMediaFileStore _mediaFileStore;

        public ImageShortCode(IMediaFileStore mediaFileStore, IHtmlSanitizerService htmlSanitizerService)
        {
            _mediaFileStore = mediaFileStore;
            _htmlSanitizerService = htmlSanitizerService;
        }

        public async override Task ProcessAsync(ShortCodeContext context, ShortCodeOutput output)
        {
            var url = await output.GetChildContentAsync();
            var publicUrl = _mediaFileStore.MapPathToPublicUrl(url.GetContent());
            var tag = "<img src=\"" + publicUrl + "\">";
            tag = _htmlSanitizerService.Sanitize(tag);

            output.Content.AppendHtml(tag);
        }
    }
}
