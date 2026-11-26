using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Twitter;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizerFactory _localizerFactory;

    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", TwitterConstants.Features.Twitter },
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
                .Add(S["Integrations"], S["Integrations"].PrefixPosition(), integrations => integrations
                    .Add(S["X (Twitter)"], S["X (Twitter)"].PrefixPosition(), twitter => twitter
                        .AddClass("twitter").Id("twitter")
                        .Action("Index", "Admin", s_routeValues)
                        .Permission(Permissions.ManageTwitter)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
