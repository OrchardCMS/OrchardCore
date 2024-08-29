using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Queries;

public sealed class AdminMenu : INavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!NavigationHelper.IsAdminMenu(name))
        {
            return Task.CompletedTask;
        }

        builder
            .Add(S["Search"], NavigationConstants.AdminMenuSearchPosition, search => search
                .AddClass("search")
                .Id("search")
                .Add(S["Queries"], S["Queries"].PrefixPosition(), queries => queries
                    .Add(S["All queries"], "1", allQueries => allQueries
                    .Action("Index", "Admin", "OrchardCore.Queries")
                    .AddClass("searchallqueries")
                    .Id("searchallqueries")
                    .Permission(Permissions.ManageQueries)
                    .LocalNav()
                )
            )
        );

        return Task.CompletedTask;
    }
}
