using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Navigation;

namespace OrchardCore.Menu.Services;

/// <summary>
/// Describes the breadcrumb trails of the menu item screens. A menu item is edited from its menu, which is a content
/// item reached from the content items list, so its trail leads back through both.
/// </summary>
public sealed class MenuBreadcrumbProvider : IBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_contentsRouteValues = new()
    {
        { "area", "OrchardCore.Contents" },
    };

    private readonly IContentManager _contentManager;

    internal readonly IStringLocalizer S;

    public MenuBreadcrumbProvider(IContentManager contentManager, IStringLocalizer<MenuBreadcrumbProvider> stringLocalizer)
    {
        _contentManager = contentManager;
        S = stringLocalizer;
    }

    public async ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case MenuBreadcrumbs.Create:
                AddManageContent(builder);
                await AddMenuAsync(builder);
                builder.Add(S["New {0}", builder.GetData<string>(MenuBreadcrumbs.ItemTypeDisplayNameKey)],
                    item => item.Id("MenuItem"));
                break;

            case MenuBreadcrumbs.Edit:
                AddManageContent(builder);
                await AddMenuAsync(builder);
                builder.Add(S["Edit {0}", builder.GetData<string>(MenuBreadcrumbs.ItemTypeDisplayNameKey)],
                    item => item.Id("MenuItem"));
                break;
        }
    }

    private void AddManageContent(BreadcrumbBuilder builder)
        => builder.Add(S["Manage Content"], item => item
            .Id("Contents")
            .Action("List", "Admin", new RouteValueDictionary(s_contentsRouteValues)
            {
                { "contentTypeId", string.Empty },
            }));

    private async ValueTask AddMenuAsync(BreadcrumbBuilder builder)
    {
        var menuContentItemId = builder.GetData<string>(MenuBreadcrumbs.MenuContentItemIdKey);

        if (string.IsNullOrEmpty(menuContentItemId))
        {
            return;
        }

        var menu = await _contentManager.GetAsync(menuContentItemId);

        builder.Add(S["Edit {0}", menu?.DisplayText], item => item
            .Id("Menu")
            .Action("Edit", "Admin", new RouteValueDictionary(s_contentsRouteValues)
            {
                { "contentItemId", menuContentItemId },
            }));
    }
}
