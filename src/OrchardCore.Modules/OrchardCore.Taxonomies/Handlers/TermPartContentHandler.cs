using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Routing;

namespace OrchardCore.Taxonomies.Handlers
{
    public class TermPartContentHandler : ContentHandlerBase
    {
        public override Task GetContentItemAspectAsync(ContentItemAspectContext context)
        {
            return context.ForAsync<ContainedContentItemsAspect>(aspect =>
            {
                // Check this content item contains Terms.
                if (context.ContentItem.Content.Terms is JArray children)
                {
                    aspect.Accessors.Add((jObject) =>
                    {
                        return jObject["Terms"] as JArray;
                    });
                }

                return Task.CompletedTask;
            });
        }
    }
}
