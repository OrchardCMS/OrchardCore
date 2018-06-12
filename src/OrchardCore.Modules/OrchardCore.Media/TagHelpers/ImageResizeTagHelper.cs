using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.Media.Processing;

namespace OrchardCore.Media.TagHelpers
{
    [HtmlTargetElement("img", Attributes = ImageSizeWidthAttributeName)]
    [HtmlTargetElement("img", Attributes = ImageSizeHeightAttributeName)]
    [HtmlTargetElement("img", Attributes = ImageSizeWidthAttributeName + "," + ImageSizeModeAttributeName)]
    [HtmlTargetElement("img", Attributes = ImageSizeHeightAttributeName + "," + ImageSizeModeAttributeName)]
    public class ImageResizeTagHelper : TagHelper
    {
        private const string ImageSizeAttributePrefix = "img-";

        private const string ImageSizeWidthAttributeName = ImageSizeAttributePrefix + "width";
        private const string ImageSizeHeightAttributeName = ImageSizeAttributePrefix + "height";
        private const string ImageSizeModeAttributeName = ImageSizeAttributePrefix + "resize-mode";

        [HtmlAttributeName(ImageSizeWidthAttributeName)]
        public int? ImageWidth { get; set; }

        [HtmlAttributeName(ImageSizeHeightAttributeName)]
        public int? ImageHeight { get; set; }

        [HtmlAttributeName(ImageSizeModeAttributeName)]
        public ResizeMode ResizeMode { get; set; }

        [HtmlAttributeName("src")]
        public string Src { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!ImageWidth.HasValue && !ImageHeight.HasValue)
            {
                return;
            }

            var imgSrc = output.Attributes["src"]?.Value.ToString() ?? Src;

            if (string.IsNullOrEmpty(imgSrc))
            {
                return;
            }

            var resizedSrc = ImageSharpUrlFormatter.GetImageResizeUrl(imgSrc, ImageWidth, ImageHeight, ResizeMode);
            output.Attributes.SetAttribute("src", resizedSrc);
        }
    }
}
