using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Deployment.Remote
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
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
                .Add(T["Content"], content => content
                    .Add(T["Import/Export"], import => import
                        .Add(T["Remote Instances"], "5.1", remote => remote
                            .Action("Index", "RemoteInstance", new { area = "OrchardCore.Deployment.Remote" })
                            .Permission(Permissions.ManageRemoteInstances)
                            .LocalNav()
                        )
                        .Add(T["Remote Clients"], remote => remote
                            .Action("Index", "RemoteClient", new { area = "OrchardCore.Deployment.Remote" })
                            .Permission(Permissions.ManageRemoteClients)
                            .LocalNav()
                        )
                    )
                );

            return Task.CompletedTask;
        }
    }
}
