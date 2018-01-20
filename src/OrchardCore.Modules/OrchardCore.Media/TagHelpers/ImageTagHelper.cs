using System.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.Media.Processing;

namespace OrchardCore.Media.TagHelpers
{
    [HtmlTargetElement("img", Attributes = ImageSrcAttributeName)]
    [HtmlTargetElement("img", Attributes = ImageSrcAttributeName + "," + ImageSizeWidthAttributeName)]
    [HtmlTargetElement("img", Attributes = ImageSrcAttributeName + "," + ImageSizeHeightAttributeName)]
    [HtmlTargetElement("img", Attributes = ImageSrcAttributeName + "," + ImageSizeWidthAttributeName + "," + ImageSizeModeAttributeName)]
    [HtmlTargetElement("img", Attributes = ImageSrcAttributeName + "," + ImageSizeHeightAttributeName + "," + ImageSizeModeAttributeName)]
    public class ImageTagHelper : TagHelper
    {
        private const string ImageSizeAttributePrefix = "media-";

        private const string ImageSrcAttributeName = ImageSizeAttributePrefix + "src";
        private const string ImageSizeWidthAttributeName = ImageSizeAttributePrefix + "width";
        private const string ImageSizeHeightAttributeName = ImageSizeAttributePrefix + "height";
        private const string ImageSizeModeAttributeName = ImageSizeAttributePrefix + "resize-mode";

        private readonly IMediaFileStore _mediaFileStore;

        public ImageTagHelper(IMediaFileStore mediaFileStore)
        {
            _mediaFileStore = mediaFileStore;
        }

        [HtmlAttributeName(ImageSizeWidthAttributeName)]
        public int? ImageWidth { get; set; }

        [HtmlAttributeName(ImageSizeHeightAttributeName)]
        public int? ImageHeight { get; set; }

        [HtmlAttributeName(ImageSizeModeAttributeName)]
        public ResizeMode ResizeMode { get; set; }

        [HtmlAttributeName(ImageSrcAttributeName)]
        public string Src { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(Src))
            {
                return;
            }

            var resolvedSrc = _mediaFileStore != null ? _mediaFileStore.MapPathToPublicUrl(Src) : Src;
            var resizedSrc = ImageSharpUrlFormatter.GetMediaResizeUrl(resolvedSrc, ImageWidth, ImageHeight, ResizeMode);

            output.Attributes.SetAttribute("src", resizedSrc);
        }
    }
}
