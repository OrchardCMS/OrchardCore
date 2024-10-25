using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.Feeds;

public sealed class ListPartFeedDisplayDriver : ContentPartDisplayDriver<ListPart>
{
    public override IDisplayResult Display(ListPart listPart, BuildPartDisplayContext context)
    {
        return Dynamic("ListPartFeed", shape =>
        {
            shape.ContentItem = listPart.ContentItem;
        })
        .Location("Detail", "Content");
    }

    public override IDisplayResult Edit(ListPart part, BuildPartEditorContext context)
    {
        return Initialize<ListFeedEditViewModel>("ListPartFeed_Edit", m =>
        {
            m.DisableRssFeed = part.ContentItem.Content.ListPart.DisableRssFeed ?? false;
            m.FeedProxyUrl = part.ContentItem.Content.ListPart.FeedProxyUrl;
            m.FeedItemsCount = part.ContentItem.Content.ListPart.FeedItemsCount ?? 20;
            m.ContentItem = part.ContentItem;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(ListPart part, UpdatePartEditorContext context)
    {
        var model = new ListFeedEditViewModel
        {
            ContentItem = part.ContentItem,
        };

        await context.Updater.TryUpdateModelAsync(model, Prefix, t => t.DisableRssFeed, t => t.FeedProxyUrl, t => t.FeedItemsCount);

        part.ContentItem.Content.ListPart.DisableRssFeed = model.DisableRssFeed;
        part.ContentItem.Content.ListPart.FeedProxyUrl = model.FeedProxyUrl;
        part.ContentItem.Content.ListPart.FeedItemsCount = model.FeedItemsCount;

        return Edit(part, context);
    }
}
