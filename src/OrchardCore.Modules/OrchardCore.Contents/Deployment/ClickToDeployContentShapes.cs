using System.Linq;
using OrchardCore.Deployment.Services;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Contents.Deployment
{
    public class ClickToDeployContentShapes : IShapeTableProvider
    {
        private readonly IDeploymentPlanService _deploymentPlanService;

        public ClickToDeployContentShapes(IDeploymentPlanService deploymentPlanService)
        {
            _deploymentPlanService = deploymentPlanService;
        }

        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("Contents_SummaryAdmin__Button__Actions")
                .OnDisplaying(async displaying =>
                {
                    if (await _deploymentPlanService.DoesUserHavePermissionsAsync())
                    {
                        var deploymentPlans = await _deploymentPlanService.GetAllDeploymentPlansAsync();
                        //TODO this could be a setting. This would store the id, then no need to retrieve all from the database.
                        var clickToDeployPlan = deploymentPlans.FirstOrDefault(x => x.DeploymentSteps.Any(x => x.Name == nameof(ClickToDeployContentDeploymentStep)));

                        if (clickToDeployPlan != null)
                        {
                            displaying.Shape.Metadata.Wrappers.Add("ClickToDeploy_Wrapper__ActionDeploymentTarget");
                        }
                    }
                });

        }
    }
}
