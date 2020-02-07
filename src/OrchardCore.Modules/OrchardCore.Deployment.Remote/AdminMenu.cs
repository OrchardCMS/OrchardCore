using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Deployment.Remote
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

            builder
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Import/Export"], import => import
                        .Add(S["Remote Instances"], "5.1", remote => remote
                            .Action("Index", "RemoteInstance", new { area = "OrchardCore.Deployment.Remote" })
                            .Permission(Permissions.ManageRemoteInstances)
                            .LocalNav()
                        )
                        .Add(S["Remote Clients"], remote => remote
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
