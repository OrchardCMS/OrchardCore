using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Indexing.Core;
using OrchardCore.Navigation;

namespace OrchardCore.Indexing.Services;

/// <summary>
/// Describes the breadcrumb trails of the indexes screens.
/// </summary>
public sealed class IndexingBreadcrumbProvider : IBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.Indexing" },
    };

    internal readonly IStringLocalizer S;

    public IndexingBreadcrumbProvider(IStringLocalizer<IndexingBreadcrumbProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case IndexingConstants.List:
                AddList(builder);
                break;

            case IndexingConstants.Create:
                AddList(builder);
                builder.Add(S["New '{0}' index", builder.GetData<string>(IndexingConstants.DisplayNameKey)],
                    item => item.Id("Index"));
                break;

            case IndexingConstants.Edit:
                AddList(builder);
                builder.Add(S["Edit '{0}' index", builder.GetData<string>(IndexingConstants.DisplayNameKey)],
                    item => item.Id("Index"));
                break;
        }

        return ValueTask.CompletedTask;
    }

    private void AddList(BreadcrumbBuilder builder)
        => builder.Add(S["Indexes"], item => item
            .Id("Indexes")
            .Action("Index", "Admin", s_routeValues)
            .Permission(IndexingPermissions.ManageIndexes));
}
