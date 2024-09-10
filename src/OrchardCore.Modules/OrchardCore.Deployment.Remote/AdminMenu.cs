using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Deployment.Remote;

public sealed class AdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
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

        return ValueTask.CompletedTask;
    }
}
