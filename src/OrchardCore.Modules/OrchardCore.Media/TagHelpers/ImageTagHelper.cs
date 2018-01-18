using System.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.Media.TagHelpers
{
    public enum ResizeMode
    {
        Undefined,
        Max,
        Crop,
        Pad,
        BoxPad,
        Min,
        Stretch
    }

    [HtmlTargetElement("img", Attributes = SrcRequiredPrefx + ImageSizeWidthAttributeName)]
    [HtmlTargetElement("img", Attributes = SrcRequiredPrefx + ImageSizeHeightAttributeName)]
    public class ImageTagHelper : TagHelper
    {
        private const string SrcRequiredPrefx = "src,";
        private const string ImageSizeAttributePrefix = "media-";

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
            if (string.IsNullOrEmpty(Src))
            {
                return;
            }

            var srcParts = Src.Split('?');
            var query = HttpUtility.ParseQueryString(srcParts.Length > 1 ? srcParts[1] : string.Empty);

            if (ImageWidth.HasValue)
            {
                query["width"] = ImageWidth.ToString();
            }

            if (ImageHeight.HasValue)
            {
                query["height"] = ImageHeight.ToString();
            }

            if (ResizeMode != ResizeMode.Undefined)
            {
                query["rmode"] = ResizeMode.ToString().ToLower();
            }

            output.Attributes.SetAttribute("src", $"{srcParts[0]}?{query.ToString()}");
        }
    }
}
