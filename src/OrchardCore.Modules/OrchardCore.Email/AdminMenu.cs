using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Email.Drivers;
using OrchardCore.Navigation;

namespace OrchardCore.Email
{
    public class AdminMenu : INavigationProvider
    {
#pragma warning disable IDE1006 // Naming Styles
        private readonly IStringLocalizer S;
#pragma warning restore IDE1006 // Naming Styles

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
                return Task.CompletedTask;

            builder
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Settings"], settings => settings
                       .Add(S["Email"], S["Email"].PrefixPosition(), entry => entry
                       .AddClass("email").Id("email")
                          .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = SmtpSettingsDisplayDriver.GroupId })
                          .Permission(Permissions.ManageEmailSettings)
                          .LocalNav()
                )));

            return Task.CompletedTask;
        }
    }
}
