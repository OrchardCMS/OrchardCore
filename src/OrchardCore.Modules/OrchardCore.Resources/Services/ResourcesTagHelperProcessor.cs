using Microsoft.Extensions.Logging;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Resources.Services;

public class ResourcesTagHelperProcessor : IResourcesTagHelperProcessor
{
    private readonly IResourceManager _resourceManager;
    private readonly ILogger<ResourcesTagHelperProcessor> _logger;

    public ResourcesTagHelperProcessor(IResourceManager resourceManager, ILogger<ResourcesTagHelperProcessor> logger)
    {
        _resourceManager = resourceManager;
        _logger = logger;
    }

    public Task ProcessAsync(ResourcesTagHelperProcessorContext context)
    {
        switch (context.Type)
        {
            case ResourceTagType.Meta:
                _resourceManager.RenderMeta(context.Writer);
                break;
            case ResourceTagType.HeadLink:
                _resourceManager.RenderHeadLink(context.Writer);
                break;
            case ResourceTagType.Stylesheet:
                _resourceManager.RenderStylesheet(context.Writer);
                break;
            case ResourceTagType.HeadScript:
                _resourceManager.RenderHeadScript(context.Writer);
                break;
            case ResourceTagType.FootScript:
                _resourceManager.RenderFootScript(context.Writer);
                break;
            case ResourceTagType.Header:
                _resourceManager.RenderMeta(context.Writer);
                _resourceManager.RenderHeadLink(context.Writer);
                _resourceManager.RenderStylesheet(context.Writer);
                _resourceManager.RenderHeadScript(context.Writer);
                break;
            case ResourceTagType.Footer:
                _resourceManager.RenderFootScript(context.Writer);
                break;
            default:
                _logger.LogWarning("Unknown {TypeName} value \"{Value}\".", nameof(ResourceTagType), context.Type);
                break;
        }

        return Task.CompletedTask;
    }
}
