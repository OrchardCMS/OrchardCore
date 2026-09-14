using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Deployment.Services;

/// <summary>
/// Describes the breadcrumb trails of the deployment screens.
/// </summary>
public sealed class DeploymentBreadcrumbProvider : IBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.Deployment" },
    };

    internal readonly IStringLocalizer S;

    public DeploymentBreadcrumbProvider(IStringLocalizer<DeploymentBreadcrumbProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case DeploymentBreadcrumbs.List:
                AddList(builder);
                break;

            case DeploymentBreadcrumbs.Create:
                AddList(builder);
                builder.Add(S["Create Deployment Plan"], item => item.Id("DeploymentPlan"));
                break;

            case DeploymentBreadcrumbs.Edit:
                AddList(builder);
                builder.Add(S["Edit Deployment Plan"], item => item.Id("DeploymentPlan"));
                break;

            case DeploymentBreadcrumbs.Display:
                AddList(builder);
                builder.Add(S["Deployment Plan"], item => item.Id("DeploymentPlan"));
                break;

            case DeploymentBreadcrumbs.StepCreate:
                AddList(builder);
                builder.Add(S["Create Step"], item => item.Id("Step"));
                break;

            case DeploymentBreadcrumbs.StepEdit:
                AddList(builder);
                builder.Add(S["Edit Step"], item => item.Id("Step"));
                break;

            // The import screens are siblings of the plans rather than children of them.
            case DeploymentBreadcrumbs.Import:
                builder.Add(S["Import Deployment Package"], item => item.Id("Import"));
                break;

            case DeploymentBreadcrumbs.ImportJson:
                builder.Add(S["JSON Import"], item => item.Id("Import"));
                break;
        }

        return ValueTask.CompletedTask;
    }

    private void AddList(BreadcrumbBuilder builder)
        => builder.Add(S["Deployment Plans"], item => item
            .Id("DeploymentPlans")
            .Action("Index", "DeploymentPlan", s_routeValues)
            .Permission(DeploymentPermissions.Export));
}
