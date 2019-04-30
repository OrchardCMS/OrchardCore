using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Media.TagHelpers
{
    [HtmlTargetElement("img", Attributes = AssetSrcAttributeName)]
    public class ImageTagHelper : TagHelper
    {
        private const string AssetSrcAttributeName = "asset-src";

        private const string AppendVersionAttributeName = "append-version";

        private readonly IMediaFileStore _mediaFileStore;

        private readonly IMediaFileStoreVersionProvider _fileStoreVersionProvider;

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
            IMemoryCache cache,
            IHttpContextAccessor httpContextAccessor,
            IMediaFileStoreVersionProvider fileStoreVersionProvider
            )
        {
            _mediaFileStore = mediaFileStore;
            _httpContextAccessor = httpContextAccessor;
            _fileStoreVersionProvider = fileStoreVersionProvider;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(AssetSrc))
            {
                return;
            }

            var resolvedSrc = _mediaFileStore != null ? _mediaFileStore.MapPathToPublicUrl(AssetSrc) : AssetSrc;
            output.Attributes.SetAttribute("src", resolvedSrc);

            if (AppendVersion && _fileStoreVersionProvider != null)
            {

                // Retrieve the TagHelperOutput variation of the "src" attribute in case other TagHelpers in the
                // pipeline have touched the value. If the value is already encoded this ImageTagHelper may
                // not function properly.
                AssetSrc = output.Attributes["src"].Value as string;

                output.Attributes.SetAttribute("src", await _fileStoreVersionProvider.AddFileVersionToPathAsync(_httpContextAccessor.HttpContext.Request.PathBase, AssetSrc));
            }
        }
    }
}
