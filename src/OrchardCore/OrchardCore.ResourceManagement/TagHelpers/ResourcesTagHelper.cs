using System;
using Cysharp.Text;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;

namespace OrchardCore.ResourceManagement.TagHelpers
{
    [HtmlTargetElement("resources", Attributes = nameof(Type))]
    public class ResourcesTagHelper : TagHelper
    {
        public ResourceTagType Type { get; set; }

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
                using var sw = new ZStringWriter();

                switch (Type)
                {
                    case ResourceTagType.Meta:
                        _resourceManager.RenderMeta(sw);
                        break;

                    case ResourceTagType.HeadLink:
                        _resourceManager.RenderHeadLink(sw);
                        break;

                    case ResourceTagType.Stylesheet:
                        _resourceManager.RenderStylesheet(sw);
                        break;

                    case ResourceTagType.HeadScript:
                        _resourceManager.RenderHeadScript(sw);
                        break;

                    case ResourceTagType.FootScript:
                        _resourceManager.RenderFootScript(sw);
                        break;

                    case ResourceTagType.Header:
                        _resourceManager.RenderMeta(sw);
                        _resourceManager.RenderHeadLink(sw);
                        _resourceManager.RenderStylesheet(sw);
                        _resourceManager.RenderHeadScript(sw);
                        break;

                    case ResourceTagType.Footer:
                        _resourceManager.RenderFootScript(sw);
                        break;

                    default:
                        _logger.LogWarning("Unknown {TypeName} value \"{Value}\".", nameof(ResourceTagType), Type);
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
