using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Seo;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizerFactory _localizerFactory;

    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", SeoConstants.RobotsSettingsGroupId },
    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer, IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(_localizerFactory.Localize(CommonLocalizationSources.Settings), settings => settings
                .Add(_localizerFactory.Localize(CommonLocalizationSources.Search), _localizerFactory.Localize(CommonLocalizationSources.Search).PrefixPosition(), search => search
                    .Add(S["Search Engine Optimization"], S["Search Engine Optimization"].PrefixPosition(), seo => seo
                        .AddClass("seo")
                        .Id("seo")
                        .Add(S["Robots"], S["Robots"].PrefixPosition(), robots => robots
                            .Action("Index", "Admin", s_routeValues)
                            .Permission(SeoConstants.ManageSeoSettings)
                            .LocalNav()
                        )
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
