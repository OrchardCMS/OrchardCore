using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Seo;

public sealed class AdminMenu : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", SeoConstants.RobotsSettingsGroupId },
    };

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
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Settings"], settings => settings
                   .Add(S["SEO"], S["SEO"].PrefixPosition(), seo => seo
                       .AddClass("seo")
                       .Id("seo")
                       .Action("Index", "Admin", _routeValues)
                       .Permission(SeoConstants.ManageSeoSettings)
                       .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
