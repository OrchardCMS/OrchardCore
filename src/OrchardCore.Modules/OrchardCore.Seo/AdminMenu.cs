using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Seo;

public class AdminMenu : INavigationProvider
{
    protected readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Settings"], settings => settings
                   .Add(S["SEO"], S["SEO"].PrefixPosition(), seo => seo
                       .AddClass("seo")
                       .Id("seo")
                       .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = SeoConstants.RobotsSettingsGroupId })
                       .Permission(SeoConstants.ManageSeoSettings)
                       .LocalNav()
                       )
                   )
                );

        return Task.CompletedTask;
    }
}
