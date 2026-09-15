using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Queries.Services;

/// <summary>
/// Describes the breadcrumb trails of the queries screens.
/// </summary>
public sealed class QueriesBreadcrumbProvider : IBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_listRouteValues = new()
    {
        { "area", "OrchardCore.Queries" },
    };

    internal readonly IStringLocalizer S;

    public QueriesBreadcrumbProvider(IStringLocalizer<QueriesBreadcrumbProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case QueriesConstants.List:
                AddList(builder);
                break;

            case QueriesConstants.Create:
                AddList(builder);
                builder.Add(S["New {0} query", builder.GetData<string>(QueriesConstants.SourceNameKey)],
                    item => item.Id("Query"));
                break;

            case QueriesConstants.Edit:
                AddList(builder);
                builder.Add(S["Edit '{0}' query", builder.GetData<string>(QueriesConstants.QueryNameKey)],
                    item => item.Id("Query"));
                break;

            case QueriesConstants.Run:
                AddList(builder);
                builder.Add(S["SQL Query"], item => item.Id("Query"));
                break;
        }

        return ValueTask.CompletedTask;
    }

    private void AddList(BreadcrumbBuilder builder)
        => builder.Add(S["Queries"], item => item
            .Id("Queries")
            .Action("Index", "Admin", s_listRouteValues)
            .Permission(QueryPermissions.ManageQueries));
}
