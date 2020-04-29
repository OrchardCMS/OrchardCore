using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment.Services;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Settings;

namespace OrchardCore.Contents.Deployment.ClickToDeploy
{
    [Feature("OrchardCore.Contents.ClickToDeploy")]
    public class ClickToDeployContentShapes : IShapeTableProvider
    {
        private readonly IDeploymentPlanService _deploymentPlanService;
        private readonly ISiteService _siteService;
        private readonly IDeploymentManager _deploymentManager;

        public ClickToDeployContentShapes(
            IDeploymentPlanService deploymentPlanService,
            ISiteService siteService,
            IDeploymentManager deploymentManager)
        {
            _deploymentPlanService = deploymentPlanService;
            _siteService = siteService;
            _deploymentManager = deploymentManager;
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

            builder.Describe("AdminBulkActions")
                .OnDisplaying(async context =>
                {
                    if (await _deploymentPlanService.DoesUserHaveExportPermissionAsync())
                    {
                        var siteSettings = await _siteService.GetSiteSettingsAsync();
                        var clickToDeploySettings = siteSettings.As<ClickToDeploySettings>();
                        if (clickToDeploySettings.ClickToDeployPlanId != 0)
                        {
                            dynamic shape = context.Shape;
                            var shapeFactory = context.ServiceProvider.GetRequiredService<IShapeFactory>();
                            var bulkActionsShape = await shapeFactory.CreateAsync("ClickToDeploy__Button__ContentsBulkAction");

                            // Don't use Items.Add() or the collection won't be sorted
                            shape.Add(bulkActionsShape, ":10");

                            var targets = await _deploymentManager.GetDeploymentTargetsAsync();
                            var targetModal = await shapeFactory.CreateAsync("ClickToDeploy_Modal__ContentsBulkActionDeploymentTarget", Arguments.From(new
                            {
                                Targets = targets,
                                clickToDeploySettings.ClickToDeployPlanId
                            }));

                            // Don't use Items.Add() or the collection won't be sorted
                            shape.Add(targetModal);
                        }
                    }
                });
        }
    }
}
