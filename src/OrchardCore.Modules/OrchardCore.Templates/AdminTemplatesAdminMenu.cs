using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Templates
{
    public class AdminTemplatesAdminMenu : INavigationProvider
    {
        public AdminTemplatesAdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(T["Configuration"], content => content
                    .Add(T["Admin Templates"], "10", import => import
                        .Action("Admin", "Template", new { area = "OrchardCore.Templates" })
                        .Permission(AdminTemplatesPermissions.ManageAdminTemplates)
                        .LocalNav()
                    )
                );

            return Task.CompletedTask;
        }
    }
}
