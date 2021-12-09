using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Templates
{
    public class AdminTemplatesAdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public AdminTemplatesAdminMenu(IStringLocalizer<AdminMenu> localizer)
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
                .Add(S["Design"], design => design
                    .Add(S["Admin Templates"], S["Admin Templates"].PrefixPosition(), import => import
                        .Action("Admin", "Template", new { area = "OrchardCore.Templates" })
                        .Permission(AdminTemplatesPermissions.ManageAdminTemplates)
                        .LocalNav()
                    )
                );

            return Task.CompletedTask;
        }
    }
}
