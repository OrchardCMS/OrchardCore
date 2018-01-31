using System.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.Media.Processing;

namespace OrchardCore.Media.TagHelpers
{
    [HtmlTargetElement("img", Attributes = ImageSrcAttributeName)]
    public class ImageTagHelper : TagHelper
    {
        private const string ImageSizeAttributePrefix = "media-";
        private const string ImageSrcAttributeName = ImageSizeAttributePrefix + "src";

        private readonly IMediaFileStore _mediaFileStore;

        public override int Order => -10;

        [HtmlAttributeName(ImageSrcAttributeName)]
        public string MediaSrc { get; set; }

        public ImageTagHelper(IMediaFileStore mediaFileStore)
        {
            _mediaFileStore = mediaFileStore;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(MediaSrc))
            {
                return;
            }

            var resolvedSrc = _mediaFileStore != null ? _mediaFileStore.MapPathToPublicUrl(MediaSrc) : MediaSrc;
            output.Attributes.SetAttribute("src", resolvedSrc);
        }
    }
}
