using Cysharp.Text;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;

namespace OrchardCore.ResourceManagement.TagHelpers;

[HtmlTargetElement("resources", Attributes = nameof(Type))]
public class ResourcesTagHelper : TagHelper
{
    private readonly ILogger _logger;
    private readonly IEnumerable<IResourcesTagHelperProcessor> _processors;

    public ResourcesTagHelper(
        ILogger<ResourcesTagHelper> logger,
        IEnumerable<IResourcesTagHelperProcessor> processors)
    {
        _logger = logger;
        _processors = processors;
    }

    public ResourceTagType Type { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        try
        {
            await using var writer = new ZStringWriter();

            var processorContext = new ResourcesTagHelperProcessorContext(Type, writer);

            foreach (var processor in _processors)
            {
                await processor.ProcessAsync(processorContext);
            }

            output.Content.AppendHtml(writer.ToString());
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
