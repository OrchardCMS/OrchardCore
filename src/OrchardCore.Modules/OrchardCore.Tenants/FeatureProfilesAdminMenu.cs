using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Navigation;

namespace OrchardCore.Tenants
{
    public class FeatureProfilesAdminMenu : INavigationProvider
    {
        private readonly ShellSettings _shellSettings;
        protected readonly IStringLocalizer S;

        public FeatureProfilesAdminMenu(IStringLocalizer<FeatureProfilesAdminMenu> localizer, ShellSettings shellSettings)
        {
            _shellSettings = shellSettings;
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            // Don't add the menu item on non-default tenants
            if (!_shellSettings.IsDefaultShell())
            {
                return Task.CompletedTask;
            }

            builder
                .Add(S["Multi-Tenancy"], tenancy => tenancy
                    .AddClass("menu-multitenancy")
                    .Add(S["Feature Profiles"], S["Feature Profiles"].PrefixPosition(), featureProfiles => featureProfiles
                        .Action("Index", "FeatureProfiles", new { area = "OrchardCore.Tenants" })
                        .Permission(Permissions.ManageTenantFeatureProfiles)
                        .LocalNav()
                    )
                );

            return Task.CompletedTask;
        }
    }
}
