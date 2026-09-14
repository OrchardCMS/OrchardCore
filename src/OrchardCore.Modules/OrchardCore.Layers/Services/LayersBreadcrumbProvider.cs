using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Layers.Services;

/// <summary>
/// Describes the breadcrumb trails of the layers screens.
/// </summary>
public sealed class LayersBreadcrumbProvider : IBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.Layers" },
    };

    internal readonly IStringLocalizer S;

    public LayersBreadcrumbProvider(IStringLocalizer<LayersBreadcrumbProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case LayersBreadcrumbs.List:
                AddList(builder);
                break;

            case LayersBreadcrumbs.Create:
                AddList(builder);
                builder.Add(S["Create Layer"], item => item.Id("Layer"));
                break;

            case LayersBreadcrumbs.Edit:
                AddList(builder);
                builder.Add(S["Edit Layer - {0}", builder.GetData<string>(LayersBreadcrumbs.LayerNameKey)],
                    item => item.Id("Layer"));
                break;

            case LayersBreadcrumbs.RuleCreate:
                AddList(builder);
                builder.Add(S["Create Rule"], item => item.Id("Rule"));
                break;

            case LayersBreadcrumbs.RuleEdit:
                AddList(builder);
                builder.Add(S["Edit Rule"], item => item.Id("Rule"));
                break;
        }

        return ValueTask.CompletedTask;
    }

    private void AddList(BreadcrumbBuilder builder)
        => builder.Add(S["Widgets and Layers"], item => item
            .Id("Layers")
            .Action("Index", "Admin", s_routeValues)
            .Permission(Permissions.ManageLayers));
}
