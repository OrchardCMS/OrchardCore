using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Features
{
    public class AdminMenu : INavigationProvider
    {
        protected readonly IStringLocalizer S;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Features"], S["Features"].PrefixPosition(), deployment => deployment
                        // Since features admin accepts tenant, always pass empty string to create valid link for current tenant.
                        .Action("Features", "Admin", new { area = FeaturesConstants.FeatureId, tenant = String.Empty })
                        .Permission(Permissions.ManageFeatures)
                        .LocalNav()
                    )
                );

            return Task.CompletedTask;
        }
    }
}
