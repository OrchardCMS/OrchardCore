using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.BackgroundTasks.Services;

/// <summary>
/// Describes the breadcrumb trails of the background tasks screens.
/// </summary>
public sealed class BackgroundTasksBreadcrumbProvider : IBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.BackgroundTasks" },
    };

    internal readonly IStringLocalizer S;

    public BackgroundTasksBreadcrumbProvider(IStringLocalizer<BackgroundTasksBreadcrumbProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case BackgroundTasksBreadcrumbs.List:
                AddList(builder);
                break;

            case BackgroundTasksBreadcrumbs.Edit:
                AddList(builder);
                builder.Add(S["Edit Task Settings"], item => item.Id("Task"));
                break;
        }

        return ValueTask.CompletedTask;
    }

    private void AddList(BreadcrumbBuilder builder)
        => builder.Add(S["Background Tasks"], item => item
            .Id("BackgroundTasks")
            .Action("Index", "BackgroundTask", s_routeValues)
            .Permission(Permissions.ManageBackgroundTasks));
}
