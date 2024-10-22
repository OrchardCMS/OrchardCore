using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Seo;

public sealed class AdminSettingsMenu : SettingsNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminSettingsMenu(IStringLocalizer<AdminSettingsMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["General"], general => general
                .Add(S["Search Engine Optimization"], S["Search Engine Optimization"].PrefixPosition(), seo => seo
                    .AddClass("seo")
                    .Id("seo")
                    .Action(GetRouteValues(SeoConstants.RobotsSettingsGroupId))
                    .Permission(SeoConstants.ManageSeoSettings)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
