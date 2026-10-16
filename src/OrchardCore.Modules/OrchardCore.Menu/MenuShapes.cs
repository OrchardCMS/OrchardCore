using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Menu.Models;
using OrchardCore.Menu.Settings;
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

                if (!menuContentItem.TryGet<MenuItemsListPart>(out var menuItemsListPart))
                {
                    return;
                }

                var menuItems = menuItemsListPart.MenuItems;

                var differentiator = FormatName(menu.GetProperty<string>("MenuName"));

                if (!string.IsNullOrEmpty(differentiator))
                {
                    // Get cached alternate and add it efficiently
                    var cachedAlternates = MenuAlternatesFactory.GetMenuAlternates(differentiator);
                    menu.Metadata.Alternates.AddRange(cachedAlternates);
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

                    var shape = await shapeFactory.CreateAsync("MenuItem", Arguments.From(new MenuItemArguments
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

                if (menuContentItem.TryGet<MenuItemsListPart>(out var menuItemsListPart))
                {
                    var permissionService = context.ServiceProvider.GetRequiredService<IPermissionService>();
                    var httpContextAccessor = context.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                    var authorizationService = context.ServiceProvider.GetRequiredService<IAuthorizationService>();
                    var contentManager = context.ServiceProvider.GetRequiredService<IContentManager>();
                    var menuItems = menuItemsListPart.MenuItems;

                    foreach (var contentItem in menuItems)
                    {
                        if (!await ShouldCreateAsync(contentItem, contentManager, permissionService, authorizationService, httpContextAccessor.HttpContext?.User))
                        {
                            continue;
                        }

                        var shape = await shapeFactory.CreateAsync("MenuItem", Arguments.From(new MenuItemArguments
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

                // Get cached alternates and add them efficiently
                var cachedAlternates = MenuItemAlternatesFactory.GetMenuItemAlternates(
                    menuContentItem.ContentItem.ContentType,
                    differentiator,
                    level);

                menuItem.Metadata.Alternates.AddRange(cachedAlternates);
            });

        builder.Describe("MenuItemLink")
            .OnDisplaying(async displaying =>
            {
                var menuItem = displaying.Shape;
                var level = menuItem.GetProperty<int>("Level");
                var differentiator = menuItem.Metadata.Differentiator;

                var menuContentItem = menuItem.GetProperty<ContentItem>("ContentItem");

                if (menuContentItem.TryGet<HtmlMenuItemPart>(out var htmlMenuItemPart))
                {
                    var contentDefinitionManager = displaying.ServiceProvider.GetRequiredService<IContentDefinitionManager>();
                    var contentTypeDefinition = await contentDefinitionManager.GetTypeDefinitionAsync(menuContentItem.ContentType);
                    var typePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(
                        part => string.Equals(part.PartDefinition.Name, nameof(HtmlMenuItemPart), StringComparison.Ordinal));
                    var settings = typePartDefinition?.GetSettings<HtmlMenuItemPartSettings>() ?? new();
                    var sanitizer = displaying.ServiceProvider.GetRequiredService<IHtmlSanitizerService>();
                    menuItem.Properties["ContentItem"] = CreateSafeMenuItem(
                        menuContentItem,
                        settings.SanitizeHtml,
                        sanitizer);
                }

                // Get cached alternates and add them efficiently
                var cachedAlternates = MenuItemAlternatesFactory.GetMenuItemLinkAlternates(
                    menuContentItem.ContentItem.ContentType,
                    differentiator,
                    level);

                menuItem.Metadata.Alternates.AddRange(cachedAlternates);
            });

        return ValueTask.CompletedTask;
    }

    internal static bool IsSafeUrl(string url, IHtmlSanitizerService sanitizer)
    {
        if (string.IsNullOrEmpty(url))
        {
            return true;
        }

        var urlToValidate = url.Split('#', 2)[0];
        if (!Uri.TryCreate(urlToValidate, UriKind.RelativeOrAbsolute, out _))
        {
            return false;
        }

        try
        {
            urlToValidate = urlToValidate.ToUriComponents();
        }
        catch (UriFormatException)
        {
            return false;
        }

        var link = $"<a href=\"{HtmlEncoder.Default.Encode(urlToValidate)}\"></a>";
        return string.Equals(link, sanitizer.Sanitize(link), StringComparison.OrdinalIgnoreCase);
    }

    internal static ContentItem CreateSafeMenuItem(
        ContentItem menuContentItem,
        bool sanitizeHtml,
        IHtmlSanitizerService sanitizer)
    {
        var sanitizedContentItem = menuContentItem.Clone();
        sanitizedContentItem.TryGet<HtmlMenuItemPart>(out var sanitizedPart);

        if (sanitizeHtml)
        {
            sanitizedPart.Html = sanitizer.Sanitize(sanitizedPart.Html ?? string.Empty);
        }

        if (!IsSafeUrl(sanitizedPart.Url, sanitizer))
        {
            sanitizedPart.Url = string.Empty;
        }

        sanitizedPart.Apply();

        return sanitizedContentItem;
    }

    private static async Task<bool> ShouldCreateAsync(
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

[GenerateArguments]
internal sealed partial class MenuItemArguments
{
    public ContentItem ContentItem { get; set; }
    public int Level { get; set; }
    public IShape Menu { get; set; }
}
