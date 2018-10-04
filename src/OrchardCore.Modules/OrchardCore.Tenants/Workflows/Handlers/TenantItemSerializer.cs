using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Environment.Shell;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public class TenantItemSerializer : IWorkflowValueSerializer
    {
        protected IShellSettingsManager _shellSettingsManager { get; }

        public TenantItemSerializer(IShellSettingsManager shellSettingsManager)
        {
            _shellSettingsManager = shellSettingsManager;
        }

        public Task DeserializeValueAsync(SerializeWorkflowValueContext context)
        {
            if (context.Input is JObject jObject)
            {
                var type = jObject.Value<string>("Type");

                //if (type == "Content")
                //{
                //    var contentId = jObject.Value<string>("ContentId");
                //    context.Output = contentId != null ? await _contentManager.GetAsync(contentId, VersionOptions.Latest) : default(IContent);
                //}
            }
            return Task.CompletedTask;
        }

        public Task SerializeValueAsync(SerializeWorkflowValueContext context)
        {
            //if (context.Input is IContent content)
            //{
            //    context.Output = JObject.FromObject(new
            //    {
            //        Type = "Content",
            //        ContentId = content.ContentItem.ContentItemId
            //    });
            //}

            return Task.CompletedTask;
        }
    }
}
