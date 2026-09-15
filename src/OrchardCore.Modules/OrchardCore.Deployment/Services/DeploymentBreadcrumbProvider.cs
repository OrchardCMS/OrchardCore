using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Mvc.Utilities;
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
    private readonly IEnumerable<IDeploymentStepFactory> _stepFactories;

    internal readonly IStringLocalizer S;

    public DeploymentBreadcrumbProvider(
        ISession session,
        IEnumerable<IDeploymentStepFactory> stepFactories,
        IStringLocalizer<DeploymentBreadcrumbProvider> stringLocalizer)
    {
        _session = session;
        _stepFactories = stepFactories;
        S = stringLocalizer;
    }

    public async ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case DeploymentConstants.List:
                AddList(builder);
                break;

            case DeploymentConstants.Create:
                AddList(builder);
                builder.Add(S["Create Deployment Plan"], item => item.Id("DeploymentPlan"));
                break;

            case DeploymentConstants.Edit:
                AddList(builder);
                builder.Add(S["Edit Deployment Plan"], item => item.Id("DeploymentPlan"));
                break;

            case DeploymentConstants.Display:
                AddList(builder);
                builder.Add(S["Deployment Plan"], item => item.Id("DeploymentPlan"));
                break;

            case DeploymentConstants.StepCreate:
                AddList(builder);
                await AddPlanAsync(builder);
                builder.Add(GetStepName(builder), item => item.Id("Step"));
                break;

            case DeploymentConstants.StepEdit:
                AddList(builder);
                await AddPlanAsync(builder);
                builder.Add(GetStepName(builder), item => item.Id("Step"));
                break;

            // The import screens are siblings of the plans rather than children of them.
            case DeploymentConstants.Import:
                builder.Add(S["Import Deployment Package"], item => item.Id("Import"));
                break;

            case DeploymentConstants.ImportJson:
                builder.Add(S["JSON Import"], item => item.Id("Import"));
                break;
        }
    }

    private void AddList(BreadcrumbBuilder builder)
        => builder.Add(S["Deployment Plans"], item => item
            .Id("DeploymentPlans")
            .Action("Index", "DeploymentPlan", s_routeValues)
            .Permission(DeploymentPermissions.Export));

    // The step's display name is authoritative, so it is read from a fresh instance of the step. A step that does not
    // set one falls back to a name derived from its type, e.g. 'ExportContentToDeploymentTargetDeploymentStep'
    // becomes 'Export Content To Deployment Target'.
    private string GetStepName(BreadcrumbBuilder builder)
    {
        var stepType = builder.GetData<string>(DeploymentConstants.StepTypeKey);

        if (string.IsNullOrEmpty(stepType))
        {
            return S["Step"].Value;
        }

        var displayName = _stepFactories.FirstOrDefault(factory => factory.Name == stepType)?.Create().Title;

        if (!string.IsNullOrEmpty(displayName?.Value))
        {
            return displayName.Value;
        }

        var derived = stepType;

        if (derived.EndsWith("DeploymentStep", StringComparison.Ordinal))
        {
            derived = derived[..^"DeploymentStep".Length];
        }
        else if (derived.EndsWith("Step", StringComparison.Ordinal))
        {
            derived = derived[..^"Step".Length];
        }

        return derived.CamelFriendly();
    }

    // A step is only reached from its deployment plan, so its trail leads back through the plan.
    private async ValueTask AddPlanAsync(BreadcrumbBuilder builder)
    {
        var planId = builder.GetData<long>(DeploymentConstants.PlanIdKey);
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
