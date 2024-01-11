using System;
using System.IO;
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
                using var sw = new StringWriter();

                switch (Type)
                {
                    case ResourceType.Meta:
                        _resourceManager.RenderMeta(sw);
                        break;

                    case ResourceType.HeadLink:
                        _resourceManager.RenderHeadLink(sw);
                        break;

                    case ResourceType.Stylesheet:
                        _resourceManager.RenderStylesheet(sw);
                        break;

                    case ResourceType.HeadScript:
                        _resourceManager.RenderHeadScript(sw);
                        break;

                    case ResourceType.FootScript:
                        _resourceManager.RenderFootScript(sw);
                        break;

                    case ResourceType.Header:
                        _resourceManager.RenderMeta(sw);
                        _resourceManager.RenderHeadLink(sw);
                        _resourceManager.RenderStylesheet(sw);
                        _resourceManager.RenderHeadScript(sw);
                        break;

                    case ResourceType.Footer:
                        _resourceManager.RenderFootScript(sw);
                        break;

                    default:
                        break;
                }

                output.Content.AppendHtml(sw.ToString());
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
