using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.Entities;
using OrchardCore.Recipes;
using OrchardCore.Recipes.Services;
using OrchardCore.Settings;

namespace OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget;

public sealed class ExportContentToDeploymentTargetMigrations : DataMigration
{
    private readonly IRecipeMigrator _recipeMigrator;
    private readonly IDeploymentPlanService _deploymentPlanService;
    private readonly ISiteService _siteService;

    public ExportContentToDeploymentTargetMigrations(
        IRecipeMigrator recipeMigrator,
        IDeploymentPlanService deploymentPlanService,
        ISiteService siteService
        )
    {
        _recipeMigrator = recipeMigrator;
        _deploymentPlanService = deploymentPlanService;
        _siteService = siteService;
    }

    public async Task<int> CreateAsync()
    {
        await _recipeMigrator.ExecuteAsync($"exportcontenttodeploymenttarget{RecipesConstants.RecipeExtension}", this).ConfigureAwait(false);

        var deploymentPlans = await _deploymentPlanService.GetAllDeploymentPlansAsync().ConfigureAwait(false);
        var exportContentToDeploymentTargetPlan = deploymentPlans.FirstOrDefault(x => x.DeploymentSteps.Any(x => x.Name == nameof(ExportContentToDeploymentTargetDeploymentStep)));

        if (exportContentToDeploymentTargetPlan != null)
        {
            var siteSettings = await _siteService.LoadSiteSettingsAsync().ConfigureAwait(false);
            siteSettings.Alter<ExportContentToDeploymentTargetSettings>(aspect => aspect.ExportContentToDeploymentTargetPlanId = exportContentToDeploymentTargetPlan.Id);

            await _siteService.UpdateSiteSettingsAsync(siteSettings).ConfigureAwait(false);
        }

        return 1;
    }
}
