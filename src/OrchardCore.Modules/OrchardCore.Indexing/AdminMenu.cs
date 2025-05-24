using Microsoft.Extensions.Localization;
using OrchardCore.Indexing.Core;
using OrchardCore.Navigation;

namespace OrchardCore.Indexing;

public sealed class AdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(
        IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Search"], NavigationConstants.AdminMenuSearchPosition, search => search
                .AddClass("search")
                .Id("search")
                .Add(S["Indexes"], S["Indexes"].PrefixPosition(), indexes => indexes
                    .Action("Index", "Admin", "OrchardCore.Indexing")
                    .AddClass("indexes")
                    .Id("indexes")
                    .Permission(IndexingPermissions.ManageIndexes)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
