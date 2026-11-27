using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Queries;

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
                .AddClass("search")
                .Id("search")
                .Add(S["Queries"], S["Queries"].PrefixPosition(), queries => queries
                    .Add(S["All Queries"], "1", allQueries => allQueries
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
