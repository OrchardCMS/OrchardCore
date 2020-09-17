using System;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger _logger;

        public ResourcesTagHelper(
            IResourceManager resourceManager,
            ILogger<ResourcesTagHelper> logger)
        {
            _resourceManager = resourceManager;
            _logger = logger;
        }

        public override void Process(TagHelperContext tagHelperContext, TagHelperOutput output)
        {
            try
            {
                switch (Type)
                {
                    case ResourceType.Meta:
                        _resourceManager.RenderMeta(output.Content);
                        break;

                    case ResourceType.HeadLink:
                        _resourceManager.RenderHeadLink(output.Content);
                        break;

                    case ResourceType.Stylesheet:
                        _resourceManager.RenderStylesheet(output.Content);
                        break;

                    case ResourceType.HeadScript:
                        _resourceManager.RenderHeadScript(output.Content);
                        break;

                    case ResourceType.FootScript:
                        _resourceManager.RenderFootScript(output.Content);
                        break;

                    case ResourceType.Header:
                        _resourceManager.RenderMeta(output.Content);
                        _resourceManager.RenderHeadLink(output.Content);
                        _resourceManager.RenderStylesheet(output.Content);
                        _resourceManager.RenderHeadScript(output.Content);
                        break;

                    case ResourceType.Footer:
                        _resourceManager.RenderFootScript(output.Content);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while rendering {Type} resource.", Type);
            }
            finally
            {
                output.TagName = null;
            }
        }
    }
}
