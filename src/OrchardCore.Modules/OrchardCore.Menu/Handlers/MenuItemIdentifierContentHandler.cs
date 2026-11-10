using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Menu.Services;

namespace OrchardCore.Menu.Handlers;

/// <summary>
/// Gives an identifier to the menu items that have none when a content item that holds menu items is saved or
/// imported. The menu admin finds a menu item by its <c>ContentItemId</c>, so it can only edit or delete menu items
/// that have one.
/// </summary>
internal sealed class MenuItemIdentifierContentHandler : ContentHandlerBase
{
    public override Task ImportingAsync(ImportContentContext context)
        => AssignMissingIdentifiersAsync(context.ContentItem);

    public override Task CreatingAsync(CreateContentContext context)
        => AssignMissingIdentifiersAsync(context.ContentItem);

    public override Task UpdatingAsync(UpdateContentContext context)
        => AssignMissingIdentifiersAsync(context.ContentItem);

    public override Task DraftSavingAsync(SaveDraftContentContext context)
        => AssignMissingIdentifiersAsync(context.ContentItem);

    public override Task PublishingAsync(PublishContentContext context)
        => AssignMissingIdentifiersAsync(context.ContentItem);

    private static Task AssignMissingIdentifiersAsync(ContentItem contentItem)
    {
        if (contentItem is not null)
        {
            MenuItemIdentifiers.AssignMissing((JsonObject)contentItem.Content, IdGenerator.GenerateId);
        }

        return Task.CompletedTask;
    }
}
