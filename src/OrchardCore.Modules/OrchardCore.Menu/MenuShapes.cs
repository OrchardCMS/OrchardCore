using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Utilities;
using OrchardCore.Menu.Models;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Menu;

public class MenuShapes : ShapeTableProvider
{
    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe("Menu")
            .OnProcessing(async context =>
            {
                var menu = context.Shape;

                // Menu population is executed when processing the shape so that its value
                // can be cached. IShapeDisplayEvents is called before the ShapeDescriptor
                // events and thus this code can be cached.

                var shapeFactory = context.ServiceProvider.GetRequiredService<IShapeFactory>();
                var contentManager = context.ServiceProvider.GetRequiredService<IContentManager>();
                var handleManager = context.ServiceProvider.GetRequiredService<IContentHandleManager>();

                var contentItemId = menu.TryGetProperty("Alias", out object alias) && alias != null
                    ? await handleManager.GetContentItemIdAsync(alias.ToString())
                    : menu.TryGetProperty("ContentItemId", out object id)
                        ? id.ToString()
                        : null;

                if (contentItemId == null)
                {
                    return;
                }

                menu.Classes.Add("menu");

                var menuContentItem = await contentManager.GetAsync(contentItemId);

                if (menuContentItem == null)
                {
                    return;
                }

                menu.Properties["ContentItem"] = menuContentItem;

                menu.Properties["MenuName"] = menuContentItem.DisplayText;

                var menuItems = menuContentItem.As<MenuItemsListPart>()?.MenuItems;

                if (menuItems == null)
                {
                    return;
                }

                var differentiator = FormatName(menu.GetProperty<string>("MenuName"));

                if (!string.IsNullOrEmpty(differentiator))
                {
                    // Menu__[MenuName] e.g. Menu-MainMenu
                    menu.Metadata.Alternates.Add("Menu__" + differentiator);
                    menu.Metadata.Differentiator = differentiator;
                    menu.Classes.Add(("menu-" + differentiator).HtmlClassify());
                }

                // The first level of menu item shapes is created.
                // Each other level is created when the menu item is displayed.

                var permissionService = context.ServiceProvider.GetRequiredService<IPermissionService>();
                var httpContextAccessor = context.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                var authorizationService = context.ServiceProvider.GetRequiredService<IAuthorizationService>();

                foreach (var contentItem in menuItems)
                {
                    if (!await ShouldCreateAsync(contentItem, contentManager, permissionService, authorizationService, httpContextAccessor.HttpContext?.User))
                    {
                        continue;
                    }

                    var shape = await shapeFactory.CreateAsync("MenuItem", Arguments.From(new
                    {
                        ContentItem = contentItem,
                        Level = 0,
                        Menu = menu,
                    }));

                    shape.Metadata.Differentiator = differentiator;

                    // Don't use Items.Add() or the collection won't be sorted
                    await ((Shape)menu).AddAsync(shape);
                }
            });

        builder.Describe("MenuItem")
            .OnDisplaying(async context =>
            {
                var menuItem = context.Shape;
                var menuContentItem = menuItem.GetProperty<ContentItem>("ContentItem");
                var menu = menuItem.GetProperty<IShape>("Menu");
                var level = menuItem.GetProperty<int>("Level");
                var differentiator = menuItem.Metadata.Differentiator;

                var shapeFactory = context.ServiceProvider.GetRequiredService<IShapeFactory>();

                var menuItems = menuContentItem.As<MenuItemsListPart>()?.MenuItems;

                if (menuItems != null)
                {
                    var permissionService = context.ServiceProvider.GetRequiredService<IPermissionService>();
                    var httpContextAccessor = context.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                    var authorizationService = context.ServiceProvider.GetRequiredService<IAuthorizationService>();
                    var contentManager = context.ServiceProvider.GetRequiredService<IContentManager>();

                    foreach (var contentItem in menuItems)
                    {
                        if (!await ShouldCreateAsync(contentItem, contentManager, permissionService, authorizationService, httpContextAccessor.HttpContext?.User))
                        {
                            continue;
                        }

                        var shape = await shapeFactory.CreateAsync("MenuItem", Arguments.From(new
                        {
                            ContentItem = contentItem,
                            Level = level + 1,
                            Menu = menu,
                        }));

                        shape.Metadata.Differentiator = differentiator;

                        // Don't use Items.Add() or the collection won't be sorted
                        await menuItem.AddAsync(shape);
                    }
                }

                // Use cached alternates and add them efficiently
                var cachedAlternates = ShapeAlternatesFactory.GetMenuItemAlternates(
                    menuContentItem.ContentItem.ContentType,
                    differentiator,
                    level);

                menuItem.Metadata.Alternates.AddRange(cachedAlternates.MenuItemAlternates);
            });

        builder.Describe("MenuItemLink")
            .OnDisplaying(displaying =>
            {
                var menuItem = displaying.Shape;
                var level = menuItem.GetProperty<int>("Level");
                var differentiator = menuItem.Metadata.Differentiator;

                var menuContentItem = menuItem.GetProperty<ContentItem>("ContentItem");

                // Use cached alternates and add them efficiently
                var cachedAlternates = ShapeAlternatesFactory.GetMenuItemAlternates(
                    menuContentItem.ContentItem.ContentType,
                    differentiator,
                    level);

                menuItem.Metadata.Alternates.AddRange(cachedAlternates.MenuItemLinkAlternates);
            });

        return ValueTask.CompletedTask;
    }

    private async static Task<bool> ShouldCreateAsync(
        ContentItem contentItem,
        IContentManager contentManager,
        IPermissionService permissionService,
        IAuthorizationService authorizationService,
        ClaimsPrincipal user)
    {
        if (contentItem.TryGet<MenuItemPermissionPart>(out var permissionPart) &&
            permissionPart.PermissionNames is not null &&
            permissionPart.PermissionNames.Length > 0)
        {
            var permissions = await permissionService.FindByNamesAsync(permissionPart.PermissionNames);

            foreach (var permission in permissions)
            {
                if (await authorizationService.AuthorizeAsync(user, permission, contentItem))
                {
                    continue;
                }

                return false;
            }
        }

        if (contentItem.TryGet<ContentMenuItemPart>(out var menuItemPart))
        {
            string contentItemId = menuItemPart.ContentItem.Content.ContentMenuItemPart.SelectedContentItem.ContentItemIds[0];

            if (string.IsNullOrEmpty(contentItemId))
            {
                return false;
            }

            if (menuItemPart.CheckContentPermissions)
            {
                var displayItem = await contentManager.GetAsync(contentItemId, VersionOptions.Published);

                if (displayItem is null)
                {
                    return false;
                }

                await contentManager.LoadAsync(displayItem);

                if (!await authorizationService.AuthorizeAsync(user, CommonPermissions.ViewContent, displayItem))
                {
                    return false;
                }
            }
        }

        return true;
    }
    /// <summary>
    /// Converts "foo-ba r" to "FooBaR".
    /// </summary>
    private static string FormatName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        name = name.Trim();
        var nextIsUpper = true;
        var result = new StringBuilder(name.Length);
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];

            if (c == '-' || char.IsWhiteSpace(c))
            {
                nextIsUpper = true;
                continue;
            }

            if (nextIsUpper)
            {
                result.Append(c.ToString().ToUpper());
            }
            else
            {
                result.Append(c);
            }

            nextIsUpper = false;
        }

        return result.ToString();
    }
}
