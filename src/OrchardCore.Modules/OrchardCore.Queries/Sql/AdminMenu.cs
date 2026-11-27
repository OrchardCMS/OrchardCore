using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Queries.Sql;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizerFactory _localizerFactory;

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer, IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(_localizerFactory.Localize(CommonLocalizationSources.Search), NavigationConstants.AdminMenuSearchPosition, search => search
                .Add(S["Queries"], S["Queries"].PrefixPosition(), queries => queries
                    .Add(S["Run SQL Query"], S["Run SQL Query"].PrefixPosition(), sql => sql
                         .Action("Query", "Admin", "OrchardCore.Queries")
                         .Permission(QueriesPermissions.ManageSqlQueries)
                         .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
