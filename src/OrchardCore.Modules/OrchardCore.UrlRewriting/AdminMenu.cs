using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.UrlRewriting;

public sealed class AdminMenu : AdminNavigationProvider
{
    public readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["URL Rewriting"], S["URL Rewriting"].PrefixPosition(), rewriting => rewriting
                    .AddClass("urlrewriting").Id("urlrewriting")
                    .Permission(UrlRewritingPermissions.ManageUrlRewriting)
                    .Action("Index", "Admin", "OrchardCore.UrlRewriting")
                    .LocalNav()
                 )
            );

        return ValueTask.CompletedTask;
    }
}
