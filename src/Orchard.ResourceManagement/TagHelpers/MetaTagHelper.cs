using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Orchard.ResourceManagement.TagHelpers
{

    [HtmlTargetElement("meta", Attributes = nameof(NameAttributeName))]
    public class MetaTagHelper : TagHelper
    {
        private const string NameAttributeName = "asp-name";

        [HtmlAttributeName(NameAttributeName)]
        public string Name { get; set; }

        public string Content { get; set; }

        public string HttpEquiv { get; set; }

        public string Charset { get; set; }

        private readonly IResourceManager _resourceManager;

        public MetaTagHelper(IResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            _resourceManager.AppendMeta(new MetaEntry(Name, Content, HttpEquiv, Charset), "");

            // We don't want any encapsulating tag around the shape
            output.TagName = null;
        }
    }
}
