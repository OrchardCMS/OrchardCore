using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using OrchardCore.Media.Fields;
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
        private const string ImageAnchorAttributeName = ImageSizeAttributePrefix + "anchor";
        private const string ImageBackgroundColorAttributeName = ImageSizeAttributePrefix + "bgcolor";

        private readonly IMediaProfileService _mediaProfileService;
        private readonly MediaOptions _mediaOptions;
        private readonly IMediaTokenService _mediaTokenService;


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

        [HtmlAttributeName(ImageAnchorAttributeName)]
        public Anchor ImageAnchor { get; set; }

        [HtmlAttributeName(ImageBackgroundColorAttributeName)]
        public string ImageBackgroundColor { get; set; }

        [HtmlAttributeName("src")]
        public string Src { get; set; }

        public ImageResizeTagHelper(
            IMediaProfileService mediaProfileService,
            IOptions<MediaOptions> mediaOptions,
            IMediaTokenService mediaTokenService)
        {
            _mediaProfileService = mediaProfileService;
            _mediaOptions = mediaOptions.Value;
            _mediaTokenService = mediaTokenService;
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

            var resizedSrc = ImageSharpUrlFormatter.GetImageResizeUrl(imgSrc, queryStringParams, ImageWidth, ImageHeight, ResizeMode, ImageQuality, ImageFormat, ImageAnchor, ImageBackgroundColor);

            if (_mediaOptions.UseTokenizedQueryString)
            {
                resizedSrc = _mediaTokenService.AddTokenToPath(resizedSrc);
            }

            output.Attributes.SetAttribute("src", resizedSrc);
        }
    }
}
