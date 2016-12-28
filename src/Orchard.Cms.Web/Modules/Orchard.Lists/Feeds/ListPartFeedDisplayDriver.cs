using System.Threading.Tasks;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Display.Models;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Feeds.Models;
using Orchard.Lists.Models;

namespace Orchard.Lists.Feeds
{
    public class ListPartFeedDisplayDriver : ContentPartDisplayDriver<ListPart>
    {
        public override IDisplayResult Display(ListPart listPart, BuildPartDisplayContext context)
        {
            return Shape("ListPart_Feed", shape =>
            {
                shape.ContentItem = listPart.ContentItem;

                return Task.CompletedTask;
            })
            .Location("Detail", "Content");
        }

        public override IDisplayResult Edit(ListPart part)
        {
            return Shape<ListFeedEditViewModel>("ListPartFeed_Edit", m =>
            {
                m.FeedProxyUrl = part.Content.FeedProxyUrl;
                m.ContentItem = part.ContentItem;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(ListPart part, IUpdateModel updater)
        {
            var model = new ListFeedEditViewModel();
            model.ContentItem = part.ContentItem;

            await updater.TryUpdateModelAsync(model, Prefix, t => t.FeedProxyUrl);

            part.Content.FeedProxyUrl = model.FeedProxyUrl;

            return Edit(part);
        }
    }
}
