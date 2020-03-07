using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows.Handlers
{
    public class BagPartHandler : ContentPartHandler<BagPart>
    {
        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, BagPart part)
        {
            return context.ForAsync<ContainedContentItemsAspect>(aspect =>
            {
                aspect.Accessors.Add((jObject) =>
                {
                    // Content.Path contains the accessor for named bag parts and typed bag parts.
                    var jContent = part.Content as JObject;
                    return jObject[jContent.Path]["ContentItems"] as JArray;
                });

                return Task.CompletedTask;
            });
        }
    }
}
