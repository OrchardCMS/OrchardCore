using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.Media.TagHelpers
{
    [HtmlTargetElement("img", Attributes = AssetSrcAttributeName)]
    public class ImageTagHelper : TagHelper
    {
        private const string AssetSrcAttributeName = "asset-src";

        private readonly IMediaFileStore _mediaFileStore;

        public override int Order => -10;

        [HtmlAttributeName(AssetSrcAttributeName)]
        public string AssetSrc { get; set; }

        public ImageTagHelper(IMediaFileStore mediaFileStore)
        {
            _mediaFileStore = mediaFileStore;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(AssetSrc))
            {
                return;
            }

            var resolvedSrc = _mediaFileStore != null ? _mediaFileStore.MapPathToPublicUrl(AssetSrc) : AssetSrc;
            output.Attributes.SetAttribute("src", resolvedSrc);
        }
    }
}
