using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Queries.Sql;

public sealed class AdminMenu : INavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    public ValueTask BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!NavigationHelper.IsAdminMenu(name))
        {
            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Search"], search => search
                .Add(S["Queries"], S["Queries"].PrefixPosition(), queries => queries
                    .Add(S["Run SQL Query"], S["Run SQL Query"].PrefixPosition(), sql => sql
                         .Action("Query", "Admin", "OrchardCore.Queries")
                         .Permission(Permissions.ManageSqlQueries)
                         .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
