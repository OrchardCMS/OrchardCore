using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Taxonomies.Models;

namespace OrchardCore.Taxonomies.Handlers
{
    public class TaxonomyPartHandler : ContentPartHandler<TaxonomyPart>
    {
        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, TaxonomyPart part)
        {
            return context.ForAsync<ContainedContentItemsAspect>(aspect =>
            {
                aspect.Accessors.Add((jObject) =>
                {
                    return jObject["TaxonomyPart"]["Terms"] as JArray;
                });

                return Task.CompletedTask;
            });
        }
    }
}
