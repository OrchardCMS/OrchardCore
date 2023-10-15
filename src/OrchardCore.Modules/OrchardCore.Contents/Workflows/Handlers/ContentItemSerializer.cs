using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public class ContentItemSerializer : IWorkflowValueSerializer
    {
        private readonly IContentManager _contentManager;

        public ContentItemSerializer(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public async Task DeserializeValueAsync(SerializeWorkflowValueContext context)
        {
            if (context.Input is JsonObject jObject)
            {
                var type = jObject["Type"]?.GetValue<string>();

                if (type == "Content")
                {
                    var contentId = jObject["ContentId"]?.GetValue<string>();
                    context.Output = contentId != null ? await _contentManager.GetAsync(contentId, VersionOptions.Latest) : default(IContent);
                }
            }
        }

        public Task SerializeValueAsync(SerializeWorkflowValueContext context)
        {
            if (context.Input is IContent content)
            {
                context.Output = JsonSerializer.SerializeToNode(new
                {
                    Type = "Content",
                    ContentId = content.ContentItem.ContentItemId
                });
            }

            return Task.CompletedTask;
        }
    }
}
