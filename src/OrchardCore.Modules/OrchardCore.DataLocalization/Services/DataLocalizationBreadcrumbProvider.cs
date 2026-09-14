using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization.Data;
using OrchardCore.Navigation;

namespace OrchardCore.DataLocalization.Services;

/// <summary>
/// Describes the breadcrumb trails of the dynamic translations screens.
/// </summary>
public sealed class DataLocalizationBreadcrumbProvider : IBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.DataLocalization" },
    };

    internal readonly IStringLocalizer S;

    public DataLocalizationBreadcrumbProvider(IStringLocalizer<DataLocalizationBreadcrumbProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case DataLocalizationBreadcrumbs.List:
                AddList(builder);
                break;

            case DataLocalizationBreadcrumbs.Statistics:
                AddList(builder);
                builder.Add(S["Statistics"], item => item.Id("Statistics"));
                break;
        }

        return ValueTask.CompletedTask;
    }

    private void AddList(BreadcrumbBuilder builder)
        => builder.Add(S["Dynamic Translations"], item => item
            .Id("DynamicTranslations")
            .Action("Index", "Admin", s_routeValues)
            .Permission(DataLocalizationPermissions.ViewDynamicTranslations));
}
