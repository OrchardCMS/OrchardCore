using System;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.ResourceManagement.TagHelpers
{
    [HtmlTargetElement("meta", Attributes = NameAttributeName)]
    [HtmlTargetElement("meta", Attributes = PropertyAttributeName)]
    public class MetaTagHelper : TagHelper
    {
        private const string NameAttributeName = "asp-name";
        private const string PropertyAttributeName = "asp-property";

        [HtmlAttributeName(NameAttributeName)]
        public string Name { get; set; }

        [HtmlAttributeName(PropertyAttributeName)]
        public string Property { get; set; }

        public string Content { get; set; }

        public string HttpEquiv { get; set; }

        public string Charset { get; set; }

        public string Separator { get; set; }

        private readonly IResourceManager _resourceManager;

        public MetaTagHelper(IResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var metaEntry = new MetaEntry(Name, Property, Content, HttpEquiv, Charset);

            foreach (var attribute in output.Attributes)
            {
                if (String.Equals(attribute.Name, "name", StringComparison.OrdinalIgnoreCase) || String.Equals(attribute.Name, "property", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                metaEntry.SetAttribute(attribute.Name, attribute.Value.ToString());
            }

            _resourceManager.AppendMeta(metaEntry, Separator ?? ", ");

            output.TagName = null;
        }
    }
}
