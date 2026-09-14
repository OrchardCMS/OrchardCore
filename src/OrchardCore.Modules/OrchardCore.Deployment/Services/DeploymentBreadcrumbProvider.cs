using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using YesSql;

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

    private readonly ISession _session;

    internal readonly IStringLocalizer S;

    public DeploymentBreadcrumbProvider(ISession session, IStringLocalizer<DeploymentBreadcrumbProvider> stringLocalizer)
    {
        _session = session;
        S = stringLocalizer;
    }

    public async ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
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
                await AddPlanAsync(builder);
                builder.Add(S["Create Step"], item => item.Id("Step"));
                break;

            case DeploymentBreadcrumbs.StepEdit:
                AddList(builder);
                await AddPlanAsync(builder);
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
    }

    private void AddList(BreadcrumbBuilder builder)
        => builder.Add(S["Deployment Plans"], item => item
            .Id("DeploymentPlans")
            .Action("Index", "DeploymentPlan", s_routeValues)
            .Permission(DeploymentPermissions.Export));

    // A step is only reached from its deployment plan, so its trail leads back through the plan.
    private async ValueTask AddPlanAsync(BreadcrumbBuilder builder)
    {
        var planId = builder.GetData<long>(DeploymentBreadcrumbs.PlanIdKey);
        var plan = await _session.GetAsync<DeploymentPlan>(planId);

        builder.Add(plan?.Name ?? S["Deployment Plan"].Value, item => item
            .Id("DeploymentPlan")
            .Action("Display", "DeploymentPlan", new RouteValueDictionary(s_routeValues)
            {
                { "id", planId },
            })
            .Permission(DeploymentPermissions.Export));
    }
}
