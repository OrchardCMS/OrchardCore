using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Menu.Models;
using OrchardCore.Menu.ViewModels;

namespace OrchardCore.Menu.Drivers;

public sealed class MenuPartDisplayDriver : ContentPartDisplayDriver<MenuPart>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly INotifier _notifier;
    private readonly ILogger _logger;

    internal readonly IHtmlLocalizer H;

    public MenuPartDisplayDriver(
        IContentDefinitionManager contentDefinitionManager,
        INotifier notifier,
        ILogger<MenuPartDisplayDriver> logger,
        IHtmlLocalizer<MenuPartDisplayDriver> htmlLocalizer
        )
    {
        _contentDefinitionManager = contentDefinitionManager;
        _notifier = notifier;
        _logger = logger;
        H = htmlLocalizer;
    }

    public override IDisplayResult Edit(MenuPart part, BuildPartEditorContext context)
    {
        return Initialize<MenuPartEditViewModel>("MenuPart_Edit", async model =>
        {
            var menuItemContentTypes = (await _contentDefinitionManager.ListTypeDefinitionsAsync())
                .Where(t => t.StereotypeEquals("MenuItem"))
                .ToArray();

            var notify = false;

            foreach (var menuItem in part.ContentItem.As<MenuItemsListPart>().MenuItems)
            {
                if (!menuItemContentTypes.Any(c => c.Name == menuItem.ContentType))
                {
                    _logger.LogWarning("The menu item content item with id {ContentItemId} has no matching {ContentType} content type definition.", menuItem.ContentItem.ContentItemId, menuItem.ContentItem.ContentType);
                    await _notifier.WarningAsync(H["The menu item content item with id {0} has no matching {1} content type definition.", menuItem.ContentItem.ContentItemId, menuItem.ContentItem.ContentType]);
                    notify = true;
                }
            }

            if (notify)
            {
                await _notifier.WarningAsync(H["Publishing this content item may erase created content. Fix any content type issues beforehand."]);
            }

            model.MenuPart = part;
            model.MenuItemContentTypes = menuItemContentTypes;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(MenuPart part, UpdatePartEditorContext context)
    {
        var model = new MenuPartEditViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix, t => t.Hierarchy);

        if (!string.IsNullOrWhiteSpace(model.Hierarchy))
        {
            var menuItems = new JsonArray();

            var originalMenuItems = part.ContentItem.As<MenuItemsListPart>();

            if (originalMenuItems is not null)
            {
                var newHierarchy = JArray.Parse(model.Hierarchy);

                foreach (var item in newHierarchy)
                {
                    menuItems.Add(ProcessItem(originalMenuItems, item as JsonObject));
                }
            }

            part.ContentItem.Content[nameof(MenuItemsListPart)] = new JsonObject
            {
                [nameof(MenuItemsListPart.MenuItems)] = menuItems,
            };
        }

        return Edit(part, context);
    }

    /// <summary>
    /// Clone the content items at the specific index.
    /// </summary>
    private static JsonObject GetMenuItemAt(MenuItemsListPart menuItems, int[] indexes)
    {
        ContentItem menuItem = null;

        foreach (var index in indexes)
        {
            menuItem = menuItems.MenuItems[index];
            menuItems = menuItem.As<MenuItemsListPart>();
        }

        var newObj = JObject.FromObject(menuItem, JOptions.Default);

        if (newObj[nameof(MenuItemsListPart)] != null)
        {
            newObj[nameof(MenuItemsListPart)] = new JsonObject
            {
                [nameof(MenuItemsListPart.MenuItems)] = new JsonArray()
            };
        }

        return newObj;
    }

    private static JsonObject ProcessItem(MenuItemsListPart originalItems, JsonObject item)
    {
        var indexes = item["index"]?.ToString().Split('-').Select(x => Convert.ToInt32(x)).ToArray() ?? [];

        var contentItem = GetMenuItemAt(originalItems, indexes);

        var children = item["children"] as JsonArray;

        if (children is not null)
        {
            var menuItems = new JsonArray();

            for (var i = 0; i < children.Count; i++)
            {
                menuItems.Add(ProcessItem(originalItems, children[i] as JsonObject));
            }

            contentItem[nameof(MenuItemsListPart)] = new JsonObject
            {
                [nameof(MenuItemsListPart.MenuItems)] = menuItems,
            };
        }

        return contentItem;
    }
}
