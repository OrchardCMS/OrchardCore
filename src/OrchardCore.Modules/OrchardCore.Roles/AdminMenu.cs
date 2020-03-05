using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Roles
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

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

            builder.Add(S["Security"], security => security
                        .Add(S["Roles"], "10", installed => installed
                            .Action("Index", "Admin", "OrchardCore.Roles")
                            .Permission(Permissions.ManageRoles)
                            .LocalNav()
                        ));

            return Task.CompletedTask;
        }
    }
}
