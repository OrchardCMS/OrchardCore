using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Seo;

public sealed class AdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", SeoConstants.RobotsSettingsGroupId },
    };

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

        builder
            .Add(S["Settings"], settings => settings
                .Add(S["Search"], S["Search"].PrefixPosition(), search => search
                    .Add(S["Search engine optimization"], S["Search engine optimization"].PrefixPosition(), seo => seo
                        .AddClass("seo")
                        .Id("seo")
                        .Add(S["Robots"], S["Robots"].PrefixPosition(), robots => robots
                            .Action("Index", "Admin", _routeValues)
                            .Permission(SeoConstants.ManageSeoSettings)
                            .LocalNav()
                        )
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
