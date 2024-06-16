using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private readonly IEnumerable<IResourcesTagHelperProcessor> _processors;

        public ResourcesTagHelper(
            IResourceManager resourceManager,
            ILogger<ResourcesTagHelper> logger,
            IEnumerable<IResourcesTagHelperProcessor> processors)
        {
            _resourceManager = resourceManager;
            _logger = logger;
            _processors = processors;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            try
            {
                await using var sw = new ZStringWriter();

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

                var processorContext = new ResourcesTagHelperProcessorContext(context, output, Type);

                foreach (var processor in _processors)
                {
                    await processor.ProcessAsync(processorContext);
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
