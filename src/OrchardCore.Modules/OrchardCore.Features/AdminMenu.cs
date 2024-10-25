using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Features;

public sealed class AdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", FeaturesConstants.FeatureId },
        // Since features admin accepts tenant, always pass empty string to create valid link for current tenant.
        { "tenant", string.Empty },
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
                .Add(S["Features"], S["Features"].PrefixPosition(), deployment => deployment
                    .Action("Features", "Admin", _routeValues)
                    .Permission(Permissions.ManageFeatures)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
