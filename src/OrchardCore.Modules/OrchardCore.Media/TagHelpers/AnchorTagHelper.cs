using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.Media.TagHelpers
{
    [HtmlTargetElement(AnchorTagName, Attributes = AssetHrefAttributeName)]
    public class AnchorTagHelper : TagHelper
    {
        private const string AssetHrefAttributeName = "asset-href";
        private const string AnchorTagName = "a";
        private const string HrefAttributeName = "href";
        private const string AppendVersionAttributeName = "asp-append-version";

        private readonly IMediaFileStore _mediaFileStore;
        private readonly IFileVersionProvider _fileVersionProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public override int Order => -10;

        [HtmlAttributeName(AssetHrefAttributeName)]
        public string AssetHref { get; set; }

        /// <summary>
        /// Value indicating if file version should be appended to href url.
        /// </summary>
        /// <remarks>
        /// If <c>true</c> then a query string "v" with the encoded content of the file is added.
        /// </remarks>
        [HtmlAttributeName(AppendVersionAttributeName)]
        public bool AppendVersion { get; set; }

        public AnchorTagHelper(
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
            if (String.IsNullOrEmpty(AssetHref))
            {
                return;
            }

            var resolvedUrl = _mediaFileStore != null ? _mediaFileStore.MapPathToPublicUrl(AssetHref) : AssetHref;

            if (AppendVersion && _fileVersionProvider != null)
            {
                output.Attributes.SetAttribute(HrefAttributeName, _fileVersionProvider.AddFileVersionToPath(_httpContextAccessor.HttpContext.Request.PathBase, resolvedUrl));
            }
            else
            {
                output.Attributes.SetAttribute(HrefAttributeName, resolvedUrl);
            }
        }
    }
}
