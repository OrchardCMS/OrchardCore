using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Deployment.Remote
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
            {
                return Task.CompletedTask;
            }

            builder
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Import/Export"], import => import
                        .Add(S["Remote Instances"], S["Remote Instances"].PrefixPosition(), remote => remote
                            .Action("Index", "RemoteInstance", new { area = "OrchardCore.Deployment.Remote" })
                            .Permission(Permissions.ManageRemoteInstances)
                            .LocalNav()
                        )
                        .Add(S["Remote Clients"], S["Remote Clients"].PrefixPosition(), remote => remote
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
