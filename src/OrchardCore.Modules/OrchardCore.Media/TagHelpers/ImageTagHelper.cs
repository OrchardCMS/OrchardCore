using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.Media.TagHelpers
{
    [HtmlTargetElement("img", Attributes = AssetSrcAttributeName)]
    [HtmlTargetElement("a", Attributes = AssetHrefAttributeName)]
    public class ImageTagHelper : TagHelper
    {
        private const string AssetSrcAttributeName = "asset-src";

        private const string AssetHrefAttributeName = "asset-href";

        private const string AppendVersionAttributeName = "asp-append-version";

        private readonly IMediaFileStore _mediaFileStore;

        private readonly IFileVersionProvider _fileVersionProvider;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public override int Order => -10;

        [HtmlAttributeName(AssetSrcAttributeName)]
        public string AssetSrc { get; set; }

        [HtmlAttributeName(AssetHrefAttributeName)]
        public string AssetHref { get; set; }

        /// <summary>
        /// Value indicating if file version should be appended to the src or href urls.
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
            string assetPath;
            string attributeName;
            if ("img".Equals(context.TagName, StringComparison.InvariantCultureIgnoreCase))
            {
                if (String.IsNullOrEmpty(AssetSrc))
                {
                    return;
                }
                assetPath = AssetSrc;
                attributeName = "src";
            }
            else if ("a".Equals(context.TagName, StringComparison.InvariantCultureIgnoreCase))
            {
                if (String.IsNullOrEmpty(AssetHref))
                {
                    return;
                }
                assetPath = AssetHref;
                attributeName = "href";
            }
            else
            {
                return;
            }

            var resolvedUrl = _mediaFileStore != null ? _mediaFileStore.MapPathToPublicUrl(assetPath) : assetPath;

            if (AppendVersion && _fileVersionProvider != null)
            {
                output.Attributes.SetAttribute(attributeName, _fileVersionProvider.AddFileVersionToPath(_httpContextAccessor.HttpContext.Request.PathBase, resolvedUrl));
            }
            else
            {
                output.Attributes.SetAttribute(attributeName, resolvedUrl);
            }
        }
    }
}
