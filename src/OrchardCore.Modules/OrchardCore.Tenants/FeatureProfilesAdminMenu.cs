using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Navigation;

namespace OrchardCore.Tenants;

public sealed class FeatureProfilesAdminMenu : AdminMenuNavigationProvider
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


    protected override void Build(NavigationBuilder builder)
    {
        // Don't add the menu item on non-default tenants.
        if (!_shellSettings.IsDefaultShell())
        {
            return;
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
    }
}
