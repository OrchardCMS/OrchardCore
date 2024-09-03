using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Facebook;

public sealed class AdminMenuPixel : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", FacebookConstants.PixelSettingsGroupId },
    };

    internal readonly IStringLocalizer S;

    public AdminMenuPixel(
        IStringLocalizer<AdminMenuLogin> stringLocalizer)
    {
        S = stringLocalizer;
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
                    .Add(S["Meta Pixel"], S["Meta Pixel"].PrefixPosition(), pixel => pixel
                        .AddClass("facebookPixel")
                        .Id("facebookPixel")
                        .Action("Index", "Admin", _routeValues)
                        .Permission(FacebookConstants.ManageFacebookPixelPermission)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
