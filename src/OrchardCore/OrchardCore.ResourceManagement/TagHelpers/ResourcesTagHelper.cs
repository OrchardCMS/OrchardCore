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

                var processorContext = new ResourcesTagHelperProcessorContext(sw, Type);

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
