using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Queries;

public sealed class AdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
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

        return ValueTask.CompletedTask;
    }
}
