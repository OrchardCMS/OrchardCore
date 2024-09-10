using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Facebook;

public sealed class AdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", FacebookConstants.Features.Core },
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
                    .Add(S["Meta App"], S["Meta App"].PrefixPosition(), metaApp => metaApp
                        .AddClass("facebookApp")
                        .Id("facebookApp")
                        .Action("Index", "Admin", _routeValues)
                        .Permission(Permissions.ManageFacebookApp)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
