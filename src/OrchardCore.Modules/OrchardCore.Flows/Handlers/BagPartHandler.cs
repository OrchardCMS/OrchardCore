using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows.Handlers
{
    public class BagPartHandler : ContentPartHandler<BagPart>
    {
        public override Task ActivatingAsync(ActivatingContentContext context, BagPart instance)
        {
            return base.ActivatingAsync(context, instance);
        }

        public override Task PublishingAsync(PublishContentContext context, BagPart instance)
        {
            return base.PublishingAsync(context, instance);
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, BagPart part)
        {
            // Content.Path contains the accessor for named bag parts
            return context.ForAsync<ContainedContentItemsAspect>(aspect =>
            {
                aspect.Accessors.Add((jObject) =>
                {
                    var jContent = part.Content as JObject;
                    return jObject[jContent.Path]["ContentItems"] as JArray;
                });

                return Task.CompletedTask;
            });
        }
    }
}
