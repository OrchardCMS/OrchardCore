using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Shortcodes.Services;

/// <summary>
/// Describes the breadcrumb trails of the shortcodes screens.
/// </summary>
public sealed class ShortcodesBreadcrumbProvider : IBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_listRouteValues = new()
    {
        { "area", "OrchardCore.Shortcodes" },
    };

    internal readonly IStringLocalizer S;

    public ShortcodesBreadcrumbProvider(IStringLocalizer<ShortcodesBreadcrumbProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case ShortcodesBreadcrumbs.List:
                AddList(builder);
                break;

            case ShortcodesBreadcrumbs.Create:
                AddList(builder);
                builder.Add(S["Create Shortcode"], item => item.Id("Shortcode"));
                break;

            case ShortcodesBreadcrumbs.Edit:
                AddList(builder);
                builder.Add(S["Edit Shortcode"], item => item.Id("Shortcode"));
                break;
        }

        return ValueTask.CompletedTask;
    }

    private void AddList(BreadcrumbBuilder builder)
        => builder.Add(S["Shortcodes"], item => item
            .Id("Shortcodes")
            .Action("Index", "Admin", s_listRouteValues)
            .Permission(ShortcodesPermissions.ManageShortcodeTemplates));
}
