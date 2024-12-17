using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Navigation;

namespace OrchardCore.Tenants;

public sealed class FeatureProfilesAdminMenu : AdminNavigationProvider
{
    private readonly ShellSettings _shellSettings;

    internal readonly IStringLocalizer S;

    public FeatureProfilesAdminMenu(
        ShellSettings shellSettings,
        IStringLocalizer<FeatureProfilesAdminMenu> stringLocalizer)
    {
        _shellSettings = shellSettings;
        S = stringLocalizer;
    }


    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        // Don't add the menu item on non-default tenants.
        if (!_shellSettings.IsDefaultShell())
        {
            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Multi-Tenancy"], tenancy => tenancy
                .AddClass("menu-multitenancy")
                .Add(S["Feature Profiles"], S["Feature Profiles"].PrefixPosition(), featureProfiles => featureProfiles
                    .Action("Index", "FeatureProfiles", "OrchardCore.Tenants")
                    .Permission(Permissions.ManageTenantFeatureProfiles)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
