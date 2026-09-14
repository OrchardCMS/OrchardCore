using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Deployment.Remote.Services;

/// <summary>
/// Describes the breadcrumb trails of the remote deployment screens.
/// </summary>
public sealed class RemoteDeploymentBreadcrumbProvider : IBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.Deployment.Remote" },
    };

    internal readonly IStringLocalizer S;

    public RemoteDeploymentBreadcrumbProvider(IStringLocalizer<RemoteDeploymentBreadcrumbProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case RemoteDeploymentConstants.Instances:
                AddInstances(builder);
                break;

            case RemoteDeploymentConstants.InstancesCreate:
                AddInstances(builder);
                builder.Add(S["Create Remote Instance"], item => item.Id("RemoteInstance"));
                break;

            case RemoteDeploymentConstants.InstancesEdit:
                AddInstances(builder);
                builder.Add(S["Edit Remote Instance"], item => item.Id("RemoteInstance"));
                break;

            case RemoteDeploymentConstants.Clients:
                AddClients(builder);
                break;

            case RemoteDeploymentConstants.ClientsEdit:
                AddClients(builder);
                builder.Add(S["Edit Remote Client"], item => item.Id("RemoteClient"));
                break;
        }

        return ValueTask.CompletedTask;
    }

    private void AddInstances(BreadcrumbBuilder builder)
        => builder.Add(S["Remote Instances"], item => item
            .Id("RemoteInstances")
            .Action("Index", "RemoteInstance", s_routeValues)
            .Permission(DeploymentPermissions.ManageRemoteInstances));

    private void AddClients(BreadcrumbBuilder builder)
        => builder.Add(S["Remote Clients"], item => item
            .Id("RemoteClients")
            .Action("Index", "RemoteClient", s_routeValues)
            .Permission(DeploymentPermissions.ManageRemoteClients));
}
