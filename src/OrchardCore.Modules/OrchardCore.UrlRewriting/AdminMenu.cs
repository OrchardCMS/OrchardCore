using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.UrlRewriting;

public sealed class AdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        if (NavigationHelper.UseLegacyFormat())
        {
            builder
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["URL Rewriting"], S["URL Rewriting"].PrefixPosition(), rewriting => rewriting
                        .AddClass("url-rewriting")
                        .Id("urlRewriting")
                        .Permission(UrlRewritingPermissions.ManageUrlRewritingRules)
                        .Action("Index", "Admin", "OrchardCore.UrlRewriting")
                        .LocalNav()
                     )
                );

            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Tools"], tools => tools
                .Add(S["URL rewriting"], S["URL Rewriting"].PrefixPosition(), rewriting => rewriting
                    .AddClass("url-rewriting")
                    .Id("urlRewriting")
                    .Permission(UrlRewritingPermissions.ManageUrlRewritingRules)
                    .Action("Index", "Admin", "OrchardCore.UrlRewriting")
                    .LocalNav()
                 )
            );

        return ValueTask.CompletedTask;
    }
}
