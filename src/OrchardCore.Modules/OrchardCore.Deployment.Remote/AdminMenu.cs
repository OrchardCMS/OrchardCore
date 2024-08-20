using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Deployment.Remote;

public sealed class AdminMenu : INavigationProvider
{
    internal readonly IStringLocalizer S;

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
                .Add(S["Import/Export"], import => import
                    .Add(S["Remote Instances"], S["Remote Instances"].PrefixPosition(), remote => remote
                        .Action("Index", "RemoteInstance", "OrchardCore.Deployment.Remote")
                        .Permission(Permissions.ManageRemoteInstances)
                        .LocalNav()
                    )
                    .Add(S["Remote Clients"], S["Remote Clients"].PrefixPosition(), remote => remote
                        .Action("Index", "RemoteClient", "OrchardCore.Deployment.Remote")
                        .Permission(Permissions.ManageRemoteClients)
                        .LocalNav()
                    )
                )
            );

        return Task.CompletedTask;
    }
}
