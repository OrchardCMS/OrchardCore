using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.ResourceManagement.TagHelpers
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
                    _resourceManager.RenderMeta(output.Content);
                    break;

                case ResourceType.HeadLink:
                    _resourceManager.RenderHeadLink(output.Content);
                    break;

                case ResourceType.Stylesheet:
                    _resourceManager.RenderStylesheet(output.Content, defaultSettings);
                    break;

                case ResourceType.HeadScript:
                    _resourceManager.RenderHeadScript(output.Content, defaultSettings);
                    break;

                case ResourceType.FootScript:
                    _resourceManager.RenderFootScript(output.Content, defaultSettings);
                    break;

                case ResourceType.Header:
                    var htmlBuilder = new HtmlContentBuilder();

                    _resourceManager.RenderMeta(output.Content);
                    _resourceManager.RenderHeadLink(output.Content);
                    _resourceManager.RenderStylesheet(output.Content, defaultSettings);
                    _resourceManager.RenderHeadScript(output.Content, defaultSettings);
                    break;

                case ResourceType.Footer:
                    _resourceManager.RenderFootScript(output.Content, defaultSettings);
                    break;

                default:
                    break;
            }

            output.TagName = null;
        }
    }
}
