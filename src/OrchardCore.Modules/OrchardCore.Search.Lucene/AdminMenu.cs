using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Search.Lucene;

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
                .Add(S["Queries"], S["Queries"].PrefixPosition(), import => import
                    .Add(S["Run Lucene Query"], S["Run Lucene Query"].PrefixPosition(), queries => queries
                        .Action("Query", "Admin", "OrchardCore.Search.Lucene")
                        .AddClass("lucenequery")
                        .Id("lucenequery")
                        .Permission(LuceneSearchPermissions.ManageLuceneIndexes)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
