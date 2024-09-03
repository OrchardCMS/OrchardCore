using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Twitter;

public sealed class AdminMenu : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", TwitterConstants.Features.Twitter },
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
