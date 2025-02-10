using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services;

public class ContentItemSerializer : IWorkflowValueSerializer
{
    private readonly IContentManager _contentManager;

    public ContentItemSerializer(IContentManager contentManager)
    {
        _contentManager = contentManager;
    }

    public async Task DeserializeValueAsync(SerializeWorkflowValueContext context)
    {
        if (context.Input is IDictionary<string, object> input)
        {
            if (input.TryGetValue("Type", out var type) && string.Equals(type as string, "Content", StringComparison.Ordinal))
            {
                context.Output = input.TryGetValue("ContentId", out var contentIdObj) && contentIdObj is string contentId
                    ? await _contentManager.GetAsync(contentId, VersionOptions.Latest)
                    : default(IContent);
            }
        }
    }

    public Task SerializeValueAsync(SerializeWorkflowValueContext context)
    {
        if (context.Input is IContent content)
        {
            context.Output = JObject.FromObject(new
            {
                Type = "Content",
                ContentId = content.ContentItem.ContentItemId
            });
        }

        return Task.CompletedTask;
    }
}
