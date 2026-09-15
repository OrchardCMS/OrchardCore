using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Placements.Services;

/// <summary>
/// Describes the breadcrumb trails of the placements screens.
/// </summary>
public sealed class PlacementsBreadcrumbProvider : IBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_listRouteValues = new()
    {
        { "area", "OrchardCore.Placements" },
    };

    internal readonly IStringLocalizer S;

    public PlacementsBreadcrumbProvider(IStringLocalizer<PlacementsBreadcrumbProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case PlacementsConstants.List:
                AddList(builder);
                break;

            case PlacementsConstants.Edit:
                AddList(builder);
                builder.Add(builder.GetData<bool>(PlacementsConstants.CreatingKey) ? S["Create Placement"] : S["Edit Placement"],
                    item => item.Id("Placement"));
                break;
        }

        return ValueTask.CompletedTask;
    }

    private void AddList(BreadcrumbBuilder builder)
        => builder.Add(S["Placements"], item => item
            .Id("Placements")
            .Action("Index", "Admin", s_listRouteValues)
            .Permission(Permissions.ManagePlacements));
}
