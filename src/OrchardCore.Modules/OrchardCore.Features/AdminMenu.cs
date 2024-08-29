using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Features;

public sealed class AdminMenu : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", FeaturesConstants.FeatureId },
        // Since features admin accepts tenant, always pass empty string to create valid link for current tenant.
        { "tenant", string.Empty },
    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!NavigationHelper.IsAdminMenu(name))
        {
            return Task.CompletedTask;
        }

        builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Features"], S["Features"].PrefixPosition(), deployment => deployment
                    .Action("Features", "Admin", _routeValues)
                    .Permission(Permissions.ManageFeatures)
                    .LocalNav()
                )
            );

        return Task.CompletedTask;
    }
}
