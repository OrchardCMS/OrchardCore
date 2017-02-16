using System;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Orchard.ResourceManagement;

namespace Orchard.Resources.TagHelpers
{

    [HtmlTargetElement("link", Attributes = SrcAttributeName)]
    public class LinkTagHelper : TagHelper
    {
        private const string SrcAttributeName = "asp-src";

        public string Rel { get; set; }

        [HtmlAttributeName(SrcAttributeName)]
        public string Href { get; set; }

        public string Title { get; set; }

        public string Type { get; set; }

        public string Condition { get; set; }

        private readonly IResourceManager _resourceManager;

        public LinkTagHelper(IResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var linkEntry = new LinkEntry();

            if (!string.IsNullOrEmpty(Href))
            {
                linkEntry.Href = Href;
            }

            if (!string.IsNullOrEmpty(Rel))
            {
                linkEntry.Rel = Rel;
            }

            if (!string.IsNullOrEmpty(Condition))
            {
                linkEntry.Condition = Condition;
            }

            if (!string.IsNullOrEmpty(Title))
            {
                linkEntry.Title = Title;
            }

            if (!string.IsNullOrEmpty(Type))
            {
                linkEntry.Type = Type;
            }

            foreach(var attribute in output.Attributes)
            {
                if (String.Equals(attribute.Name, "href", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                linkEntry.SetAttribute(attribute.Name, attribute.Value.ToString());
            }

            _resourceManager.RegisterLink(linkEntry);

            output.TagName = null;
        }
    }
}
