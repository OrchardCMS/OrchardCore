using OrchardCore.Deployment.Services;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Contents.Deployment.ClickToDeploy
{
    public class ClickToDeployContentShapes : IShapeTableProvider
    {
        private readonly IDeploymentPlanService _deploymentPlanService;
        private readonly ISiteService _siteService;

        public ClickToDeployContentShapes(IDeploymentPlanService deploymentPlanService, ISiteService siteService)
        {
            _deploymentPlanService = deploymentPlanService;
            _siteService = siteService;
        }

        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("Contents_SummaryAdmin__Button__Actions")
                .OnDisplaying(async displaying =>
                {
                    if (await _deploymentPlanService.DoesUserHaveExportPermissionAsync())
                    {
                        var siteSettings = await _siteService.GetSiteSettingsAsync();
                        var clickToDeploySettings = siteSettings.As<ClickToDeploySettings>();
                        if (clickToDeploySettings.ClickToDeployPlanId != 0)
                        {
                            displaying.Shape.Metadata.Wrappers.Add("ClickToDeploy_Wrapper__ActionDeploymentTarget");
                        }
                    }
                });

        }
    }
}
