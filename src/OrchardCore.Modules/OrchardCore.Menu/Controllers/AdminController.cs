using System.Text.Json.Nodes;
using System.Text.Json.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Menu.Models;
using YesSql;

namespace OrchardCore.Menu.Controllers;

[Admin("Menu/{action}/{id?}", "Menu{action}")]
public sealed class AdminController : Controller
{
    private readonly IContentManager _contentManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentItemDisplayManager _contentItemDisplayManager;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly ISession _session;
    private readonly INotifier _notifier;
    private readonly IUpdateModelAccessor _updateModelAccessor;

    internal readonly IHtmlLocalizer H;

    public AdminController(
        ISession session,
        IContentManager contentManager,
        IAuthorizationService authorizationService,
        IContentItemDisplayManager contentItemDisplayManager,
        IContentDefinitionManager contentDefinitionManager,
        INotifier notifier,
        IHtmlLocalizer<AdminController> localizer,
        IUpdateModelAccessor updateModelAccessor)
    {
        _contentManager = contentManager;
        _authorizationService = authorizationService;
        _contentItemDisplayManager = contentItemDisplayManager;
        _contentDefinitionManager = contentDefinitionManager;
        _session = session;
        _notifier = notifier;
        _updateModelAccessor = updateModelAccessor;
        H = localizer;
    }

    public async Task<IActionResult> Create(string id, string menuContentItemId, string menuItemId)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMenu))
        {
            return Forbid();
        }

        var contentItem = await _contentManager.NewAsync(id);

        var model = await _contentItemDisplayManager.BuildEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, true);

        model.Properties["MenuContentItemId"] = menuContentItemId;
        model.Properties["MenuItemId"] = menuItemId;

        return View(model);
    }

    [HttpPost]
    [ActionName("Create")]
    public async Task<IActionResult> CreatePost(string id, string menuContentItemId, string menuItemId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMenu))
        {
            return Forbid();
        }

        ContentItem menu;

        var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync("Menu");

        if (!contentTypeDefinition.IsDraftable())
        {
            menu = await _contentManager.GetAsync(menuContentItemId, VersionOptions.Latest);
        }
        else
        {
            menu = await _contentManager.GetAsync(menuContentItemId, VersionOptions.DraftRequired);
        }

        if (menu == null)
        {
            return NotFound();
        }

        var contentItem = await _contentManager.NewAsync(id);

        var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, true);

        if (!ModelState.IsValid)
        {
            model.Properties["MenuContentItemId"] = menuContentItemId;
            model.Properties["MenuItemId"] = menuItemId;

            return View(model);
        }

        if (menuItemId == null)
        {
            // Use the menu as the parent if no target is specified.
            menu.Alter<MenuItemsListPart>(part => part.MenuItems.Add(contentItem));
        }
        else
        {
            // Look for the target menu item in the hierarchy.
            var parentMenuItem = FindMenuItem((JsonObject)menu.Content, menuItemId);

            // Couldn't find targeted menu item.
            if (parentMenuItem == null)
            {
                return NotFound();
            }

            var menuItems = (JsonArray)parentMenuItem["MenuItemsListPart"]?["MenuItems"];

            if (menuItems == null)
            {
                parentMenuItem["MenuItemsListPart"] = new JsonObject
                {
                    ["MenuItems"] = menuItems = [],
                };
            }

            menuItems.Add(JObject.FromObject(contentItem));
        }

        await _contentManager.SaveDraftAsync(menu);

        return RedirectToAction(nameof(Edit), "Admin", new { area = "OrchardCore.Contents", contentItemId = menuContentItemId });
    }

    public async Task<IActionResult> Edit(string menuContentItemId, string menuItemId)
    {
        var menu = await _contentManager.GetAsync(menuContentItemId, VersionOptions.Latest);

        if (menu == null)
        {
            return NotFound();
        }

        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMenu, menu))
        {
            return Forbid();
        }

        // Look for the target menu item in the hierarchy.
        var menuItem = FindMenuItem((JsonObject)menu.Content, menuItemId);

        // Couldn't find targeted menu item.
        if (menuItem == null)
        {
            return NotFound();
        }

        var contentItem = menuItem.ToObject<ContentItem>();

        var model = await _contentItemDisplayManager.BuildEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false);

        model.Properties["MenuContentItemId"] = menuContentItemId;
        model.Properties["MenuItemId"] = menuItemId;

        return View(model);
    }

    [HttpPost]
    [ActionName("Edit")]
    public async Task<IActionResult> EditPost(string menuContentItemId, string menuItemId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMenu))
        {
            return Forbid();
        }

        ContentItem menu;

        var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync("Menu");

        if (!contentTypeDefinition.IsDraftable())
        {
            menu = await _contentManager.GetAsync(menuContentItemId, VersionOptions.Latest);
        }
        else
        {
            menu = await _contentManager.GetAsync(menuContentItemId, VersionOptions.DraftRequired);
        }

        if (menu == null)
        {
            return NotFound();
        }

        // Look for the target menu item in the hierarchy.
        var menuItem = FindMenuItem((JsonObject)menu.Content, menuItemId);

        // Couldn't find targeted menu item
        if (menuItem == null)
        {
            return NotFound();
        }

        var existing = menuItem.ToObject<ContentItem>();

        // Create a new item to take into account the current type definition.
        var contentItem = await _contentManager.NewAsync(existing.ContentType);

        contentItem.Merge(existing);

        var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false);

        if (!ModelState.IsValid)
        {
            model.Properties["MenuContentItemId"] = menuContentItemId;
            model.Properties["MenuItemId"] = menuItemId;

            return View(model);
        }

        menuItem.Merge((JsonObject)contentItem.Content, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Replace,
            MergeNullValueHandling = MergeNullValueHandling.Merge
        });

        // Merge doesn't copy the properties.
        menuItem[nameof(ContentItem.DisplayText)] = contentItem.DisplayText;

        await _contentManager.SaveDraftAsync(menu);

        return RedirectToAction(nameof(Edit), "Admin", new { area = "OrchardCore.Contents", contentItemId = menuContentItemId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string menuContentItemId, string menuItemId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMenu))
        {
            return Forbid();
        }

        var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync("Menu");
        var menu = contentTypeDefinition.IsDraftable()
            ? await _contentManager.GetAsync(menuContentItemId, VersionOptions.DraftRequired)
            : await _contentManager.GetAsync(menuContentItemId, VersionOptions.Latest);

        if (menu == null)
        {
            return NotFound();
        }

        var menuContentAsJson = (JsonObject)menu.Content;
        // Look for the target menu item in the hierarchy.
        var menuItem = FindMenuItem(menuContentAsJson, menuItemId);

        // Couldn't find targeted menu item.
        if (menuItem == null)
        {
            return NotFound();
        }

        var menuItems = menuContentAsJson.SelectNode(menuItem.Parent.GetPath()) as JsonArray;

        menuItems.Remove(menuItem);

        await _contentManager.SaveDraftAsync(menu);

        await _notifier.SuccessAsync(H["Menu item deleted successfully."]);

        return RedirectToAction(nameof(Edit), "Admin", new { area = "OrchardCore.Contents", contentItemId = menuContentItemId });
    }

    private static JsonObject FindMenuItem(JsonObject contentItem, string menuItemId)
    {
        if (contentItem["ContentItemId"]?.Value<string>() == menuItemId)
        {
            return contentItem;
        }

        if (contentItem["MenuItemsListPart"] is null)
        {
            return null;
        }

        var menuItems = (JsonArray)contentItem["MenuItemsListPart"]["MenuItems"];

        JsonObject result;
        foreach (var menuItem in menuItems.Cast<JsonObject>())
        {
            // Search in inner menu items.
            result = FindMenuItem(menuItem, menuItemId);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}
