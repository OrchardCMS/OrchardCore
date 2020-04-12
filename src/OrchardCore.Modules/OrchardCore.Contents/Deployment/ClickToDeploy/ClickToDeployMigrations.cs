using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment.Services;
using OrchardCore.Entities;
using OrchardCore.Recipes.Services;
using OrchardCore.Settings;

namespace OrchardCore.Contents.Deployment.ClickToDeploy
{
    public class ClickToDeployMigrations : DataMigration
    {
        private readonly IRecipeMigrator _recipeMigrator;
        private readonly IDeploymentPlanService _deploymentPlanService;
        private readonly ISiteService _siteService;

        public ClickToDeployMigrations(
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
            await _recipeMigrator.ExecuteAsync("clicktodeploy.recipe.json", this);

            var deploymentPlans = await _deploymentPlanService.GetAllDeploymentPlansAsync();
            var clickToDeployPlan = deploymentPlans.FirstOrDefault(x => x.DeploymentSteps.Any(x => x.Name == nameof(ClickToDeployContentDeploymentStep)));

            if (clickToDeployPlan != null)
            {
                var siteSettings = await _siteService.LoadSiteSettingsAsync();
                siteSettings.Alter<ClickToDeploySettings>(nameof(ClickToDeploySettings), aspect => aspect.ClickToDeployPlanId = clickToDeployPlan.Id);

                await _siteService.UpdateSiteSettingsAsync(siteSettings);
            }

            return 1;
        }
    }
}
