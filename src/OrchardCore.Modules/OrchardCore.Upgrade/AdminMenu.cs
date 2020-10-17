using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Security;

namespace OrchardCore.Upgrade
{
    public class UserIdAdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public UserIdAdminMenu(IStringLocalizer<UserIdAdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder.Add(S["Upgrade Users from RC2"], "120", security => security
                    .AddClass("upgrade").Id("upgrade")
                    .Action("Index", "UserId", "OrchardCore.Upgrade")
                    .Permission(StandardPermissions.SiteOwner)
                    .LocalNav());

            return Task.CompletedTask;
        }
    }
}
