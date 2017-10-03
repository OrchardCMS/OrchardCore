using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OrchardCore.ContentManagement;
using OrchardCore.RestApis;
using OrchardCore.RestApis.ContentDisplay;
using OrchardCore.Lists.Indexes;
using OrchardCore.Lists.Models;
using YesSql;

namespace OrchardCore.Lists.Drivers
{
    public class ListPartApiPartDriver : IApiPartDriver
    {
        private readonly ISession _session;

        public ListPartApiPartDriver(ISession session)
        {
            _session = session;
        }

        public async Task Apply(ContentPart part, BuildApiDisplayContext context)
        {
            var listPart = (ListPart)part;

            var query = _session.Query<ContentItem>()
                    .With<ContainedPartIndex>(x => x.ListContentItemId == listPart.ContentItem.ContentItemId);

            var containedItems = await query.ListAsync();

            var counter = containedItems.Count();

            foreach (var contentItem in containedItems)
            {
                var relationship = new ApiRelationshipItem(part.ContentItem, contentItem, context.UrlHelper);

                relationship.Meta.Add("count", JsonConvert.SerializeObject(counter));

                context.Item.AddRelationship(
                    relationship
                );
            }
        }

        public bool CanApply(ContentPart part)
        {
            return part.GetType() == typeof(ListPart);
        }
    }
}
