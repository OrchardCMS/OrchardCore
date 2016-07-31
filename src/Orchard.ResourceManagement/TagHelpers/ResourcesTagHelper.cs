using System;
using System.Linq;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Orchard.ResourceManagement.TagHelpers
{
    public enum ResourceType
    {
        Meta,
        HeadLink,
        Stylesheet,
        HeadScript,
        FootScript,
        Header,
        Footer
    }

    [HtmlTargetElement("resources", Attributes = nameof(Type))]
    public class ResourcesTagHelper : TagHelper
    {
        public ResourceType Type { get; set; }

        private readonly IResourceManager _resourceManager;
        private readonly IRequireSettingsProvider _requireSettingsProvider;

        public ResourcesTagHelper(IResourceManager resourceManager, IRequireSettingsProvider requireSettingsProvider)
        {
            _requireSettingsProvider = requireSettingsProvider;
            _resourceManager = resourceManager;
        }

        public override void Process(TagHelperContext tagHelperContext, TagHelperOutput output)
        {
            var defaultSettings = _requireSettingsProvider.GetDefault();

            switch (Type)
            {
                case ResourceType.Meta:
                    output.Content.SetHtmlContent(_resourceManager.RenderMeta());
                    break;

                case ResourceType.HeadLink:
                    output.Content.SetHtmlContent(_resourceManager.RenderHeadLink());
                    break;

                case ResourceType.Stylesheet:
                    output.Content.SetHtmlContent(_resourceManager.RenderStylesheet(defaultSettings));
                    break;

                case ResourceType.HeadScript:
                    output.Content.SetHtmlContent(_resourceManager.RenderHeadScript(defaultSettings));
                    break;

                case ResourceType.FootScript:
                    output.Content.SetHtmlContent(_resourceManager.RenderFootScript(defaultSettings));
                    break;

                case ResourceType.Header:
                    output.Content.SetHtmlContent(_resourceManager.RenderMeta());
                    output.Content.SetHtmlContent(_resourceManager.RenderHeadLink());
                    output.Content.SetHtmlContent(_resourceManager.RenderStylesheet(defaultSettings));
                    output.Content.SetHtmlContent(_resourceManager.RenderHeadScript(defaultSettings));
                    break;

                case ResourceType.Footer:
                    output.Content.SetHtmlContent(_resourceManager.RenderFootScript(defaultSettings));
                    break;

                default:
                    break;
            }

            output.TagName = null;
        }
    }
}
