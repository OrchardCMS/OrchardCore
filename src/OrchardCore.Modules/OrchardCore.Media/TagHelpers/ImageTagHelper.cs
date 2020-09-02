using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.Media.TagHelpers
{
    [HtmlTargetElement(ImageTagName, Attributes = AssetSrcAttributeName)]
    public class ImageTagHelper : TagHelper
    {
        private const string AssetSrcAttributeName = "asset-src";

        private const string ImageTagName = "img";

        private const string SourceAttributeName = "src";

        private const string AppendVersionAttributeName = "asp-append-version";

        private readonly IMediaFileStore _mediaFileStore;

        private readonly IFileVersionProvider _fileVersionProvider;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public override int Order => -10;

        [HtmlAttributeName(AssetSrcAttributeName)]
        public string AssetSrc { get; set; }

        /// <summary>
        /// Value indicating if file version should be appended to the src url.
        /// </summary>
        /// <remarks>
        /// If <c>true</c> then a query string "v" with the encoded content of the file is added.
        /// </remarks>
        [HtmlAttributeName(AppendVersionAttributeName)]
        public bool AppendVersion { get; set; }

        public ImageTagHelper(
            IMediaFileStore mediaFileStore,
            IHttpContextAccessor httpContextAccessor,
            IFileVersionProvider fileVersionProvider
            )
        {
            _mediaFileStore = mediaFileStore;
            _httpContextAccessor = httpContextAccessor;
            _fileVersionProvider = fileVersionProvider;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (String.IsNullOrEmpty(AssetSrc))
            {
                return;
            }

            var resolvedUrl = _mediaFileStore != null ? _mediaFileStore.MapPathToPublicUrl(AssetSrc) : AssetSrc;

            if (AppendVersion && _fileVersionProvider != null)
            {
                output.Attributes.SetAttribute(SourceAttributeName, _fileVersionProvider.AddFileVersionToPath(_httpContextAccessor.HttpContext.Request.PathBase, resolvedUrl));
            }
            else
            {
                output.Attributes.SetAttribute(SourceAttributeName, resolvedUrl);
            }
        }
    }
}
