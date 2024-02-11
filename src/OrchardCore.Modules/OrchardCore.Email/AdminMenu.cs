using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Email.Drivers;
using OrchardCore.Navigation;

namespace OrchardCore.Email
{
    public class AdminMenu : INavigationProvider
    {
        private static readonly RouteValueDictionary _routeValues = new()
        {
            { "area", "OrchardCore.Settings" },
            { "groupId", SmtpSettingsDisplayDriver.GroupId },
        };

        protected readonly IStringLocalizer S;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!NavigationHelper.IsAdminMenu(name))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Settings"], settings => settings
                       .Add(S["Email"], S["Email"].PrefixPosition(), entry => entry
                          .AddClass("email").Id("email")
                          .Action("Index", "Admin", _routeValues)
                          .Permission(Permissions.ManageEmailSettings)
                          .LocalNav()
                        )
                    )
                );

            return Task.CompletedTask;
        }
    }
}
