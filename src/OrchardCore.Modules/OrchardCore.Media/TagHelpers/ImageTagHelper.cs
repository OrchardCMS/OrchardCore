using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.Media.TagHelpers
{
    [HtmlTargetElement("img", Attributes = AssetSrcAttributeName)]
    public class ImageTagHelper : TagHelper
    {
        private const string AssetSrcAttributeName = "asset-src";

        private const string AppendVersionAttributeName = "asp-append-version";

        private readonly IMediaFileStore _mediaFileStore;

        private readonly IFileVersionProvider _fileVersionProvider;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public override int Order => -10;

        [HtmlAttributeName(AssetSrcAttributeName)]
        public string AssetSrc { get; set; }

        /// <summary>
        /// Value indicating if file version should be appended to the src urls.
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
            if (string.IsNullOrEmpty(AssetSrc))
            {
                return;
            }

            var resolvedSrc = _mediaFileStore != null ? _mediaFileStore.MapPathToPublicUrl(AssetSrc) : AssetSrc;
            output.Attributes.SetAttribute("src", resolvedSrc);

            if (AppendVersion && _fileVersionProvider != null)
            {
                output.Attributes.SetAttribute("src", _fileVersionProvider.AddFileVersionToPath(_httpContextAccessor.HttpContext.Request.PathBase, resolvedSrc));
            }
        }
    }
}
