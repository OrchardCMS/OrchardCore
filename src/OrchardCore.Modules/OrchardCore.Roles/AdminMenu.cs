using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Roles
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder.Add(T["Security"], security => security
                        .Add(T["Roles"], "10", installed => installed
                            .Action("Index", "Admin", "OrchardCore.Roles")
                            .Permission(Permissions.ManageRoles)
                            .LocalNav()
                        ));

            return Task.CompletedTask;
        }
    }
}
