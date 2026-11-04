using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.RemoteManagement;

namespace OrchardCore.OpenId;

public sealed class RemoteManagementAdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public RemoteManagementAdminMenu(IStringLocalizer<RemoteManagementAdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Settings"], settings => settings
                .Add(S["Remote Management"], S["Remote Management"].PrefixPosition(), remoteManagement => remoteManagement
                    .Action("Index", "RemoteManagement", "OrchardCore.OpenId")
                    .Permission(RemoteManagementPermissions.ManageRemoteManagementConfiguration)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
