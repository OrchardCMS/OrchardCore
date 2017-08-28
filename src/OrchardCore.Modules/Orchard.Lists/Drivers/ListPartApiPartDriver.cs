using System.Threading.Tasks;
using Orchard.ContentManagement;
using Orchard.JsonApi;
using Orchard.JsonApi.ContentDisplay;
using Orchard.Lists.Indexes;
using Orchard.Lists.Models;
using YesSql;

namespace Orchard.Lists.Drivers
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

            foreach (var contentItem in containedItems)
            {
                context.Item.AddRelationship(
                    new ApiRelationshipItem(part.ContentItem, contentItem, context.UrlHelper)
                );
            }
        }

        public bool CanApply(ContentPart part)
        {
            return part.GetType() == typeof(ListPart);
        }
    }
}
