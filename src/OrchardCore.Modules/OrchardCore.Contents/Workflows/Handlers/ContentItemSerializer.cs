using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
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
            if (context.Input is JObject jObject)
            {
                dynamic value = jObject;
                var type = value.Type as string;

                if (type == "Content")
                {
                    var contentId = value.ContentId as string;
                    context.Output = contentId != null ? await _contentManager.GetAsync(contentId, VersionOptions.Latest) : default(IContent);
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
}
