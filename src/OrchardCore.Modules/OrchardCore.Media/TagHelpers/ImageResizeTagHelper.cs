using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.Media.Processing;
using OrchardCore.Media.Services;

namespace OrchardCore.Media.TagHelpers
{
    [HtmlTargetElement("img", Attributes = ImageSizeWidthAttributeName)]
    [HtmlTargetElement("img", Attributes = ImageSizeHeightAttributeName)]
    [HtmlTargetElement("img", Attributes = ImageProfileAttributeName)]
    [HtmlTargetElement("img", Attributes = ImageSizeWidthAttributeName + "," + ImageSizeModeAttributeName)]
    [HtmlTargetElement("img", Attributes = ImageSizeHeightAttributeName + "," + ImageSizeModeAttributeName)]
    public class ImageResizeTagHelper : TagHelper
    {
        private const string ImageSizeAttributePrefix = "img-";
        private const string ImageSizeWidthAttributeName = ImageSizeAttributePrefix + "width";
        private const string ImageSizeHeightAttributeName = ImageSizeAttributePrefix + "height";
        private const string ImageSizeModeAttributeName = ImageSizeAttributePrefix + "resize-mode";
        private const string ImageQualityAttributeName = ImageSizeAttributePrefix + "quality";
        private const string ImageFormatAttributeName = ImageSizeAttributePrefix + "format";
        private const string ImageProfileAttributeName = ImageSizeAttributePrefix + "profile";

        private readonly IMediaProfileService _mediaProfileService;


        [HtmlAttributeName(ImageSizeWidthAttributeName)]
        public int? ImageWidth { get; set; }

        [HtmlAttributeName(ImageSizeHeightAttributeName)]
        public int? ImageHeight { get; set; }

        [HtmlAttributeName(ImageQualityAttributeName)]
        public int? ImageQuality { get; set; }

        [HtmlAttributeName(ImageSizeModeAttributeName)]
        public ResizeMode ResizeMode { get; set; }

        [HtmlAttributeName(ImageFormatAttributeName)]
        public Format ImageFormat { get; set; }

        [HtmlAttributeName(ImageProfileAttributeName)]
        public string ImageProfile { get; set; }

        [HtmlAttributeName("src")]
        public string Src { get; set; }

        public ImageResizeTagHelper(IMediaProfileService mediaProfileService)
        {
            _mediaProfileService = mediaProfileService;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!ImageWidth.HasValue && !ImageHeight.HasValue && String.IsNullOrEmpty(ImageProfile))
            {
                return;
            }

            var imgSrc = output.Attributes["src"]?.Value.ToString() ?? Src;

            if (string.IsNullOrEmpty(imgSrc))
            {
                return;
            }

            IDictionary<string, string> queryStringParams = null;

            if (!String.IsNullOrEmpty(ImageProfile))
            {
                queryStringParams = await _mediaProfileService.GetMediaProfileCommands(ImageProfile);
            }

            var resizedSrc = ImageSharpUrlFormatter.GetImageResizeUrl(imgSrc, queryStringParams, ImageWidth, ImageHeight, ResizeMode, ImageQuality, ImageFormat);

            output.Attributes.SetAttribute("src", resizedSrc);
        }
    }
}
