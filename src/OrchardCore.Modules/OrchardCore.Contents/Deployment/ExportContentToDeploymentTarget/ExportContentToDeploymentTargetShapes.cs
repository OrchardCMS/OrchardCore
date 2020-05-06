using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.Deployment.Services;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Settings;

namespace OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget
{
    [Feature("OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget")]
    public class ExportContentToDeploymentTargetShapes : IShapeTableProvider
    {
        private readonly IDeploymentPlanService _deploymentPlanService;
        private readonly ISiteService _siteService;
        private readonly IDeploymentManager _deploymentManager;

        public ExportContentToDeploymentTargetShapes(
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
                        var exportContentToDeploymentTargetSettings = siteSettings.As<ExportContentToDeploymentTargetSettings>();
                        if (exportContentToDeploymentTargetSettings.ExportContentToDeploymentTargetPlanId != 0)
                        {
                            displaying.Shape.Metadata.Wrappers.Add("ExportContentToDeploymentTarget_Wrapper__ActionDeploymentTarget");
                        }
                    }
                });

            builder.Describe("AdminBulkActions")
                .OnDisplaying(async context =>
                {
                    if (await _deploymentPlanService.DoesUserHaveExportPermissionAsync())
                    {
                        var siteSettings = await _siteService.GetSiteSettingsAsync();
                        var exportContentToDeploymentTargetSettings = siteSettings.As<ExportContentToDeploymentTargetSettings>();
                        if (exportContentToDeploymentTargetSettings.ExportContentToDeploymentTargetPlanId != 0)
                        {
                            dynamic shape = context.Shape;
                            var shapeFactory = context.ServiceProvider.GetRequiredService<IShapeFactory>();
                            var bulkActionsShape = await shapeFactory.CreateAsync("ExportContentToDeploymentTarget__Button__ContentsBulkAction");

                            // Don't use Items.Add() or the collection won't be sorted
                            shape.Add(bulkActionsShape, ":10");

                            var targets = await _deploymentManager.GetDeploymentTargetsAsync();
                            var targetModal = await shapeFactory.CreateAsync("ExportContentToDeploymentTarget_Modal__ContentsBulkActionDeploymentTarget", Arguments.From(new
                            {
                                Targets = targets,
                                exportContentToDeploymentTargetSettings.ExportContentToDeploymentTargetPlanId
                            }));

                            // Don't use Items.Add() or the collection won't be sorted
                            shape.Add(targetModal);
                        }
                    }
                });
        }
    }
}
