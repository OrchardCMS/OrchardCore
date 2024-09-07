using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Twitter;

public sealed class AdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", TwitterConstants.Features.Twitter },
    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Settings"], settings => settings
                    .Add(S["X (Twitter)"], S["X (Twitter)"].PrefixPosition(), twitter => twitter
                        .AddClass("twitter").Id("twitter")
                        .Action("Index", "Admin", _routeValues)
                        .Permission(Permissions.ManageTwitter)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
