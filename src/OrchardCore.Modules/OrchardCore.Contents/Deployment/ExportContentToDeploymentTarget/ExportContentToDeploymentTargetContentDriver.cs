using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget
{
    public class ExportContentToDeploymentTargetContentDriver : ContentDisplayDriver
    {
        private readonly IDeploymentPlanService _deploymentPlanService;
        private readonly ISiteService _siteService;

        public ExportContentToDeploymentTargetContentDriver(
            IDeploymentPlanService deploymentPlanService,
            ISiteService siteService)
        {
            _deploymentPlanService = deploymentPlanService;
            _siteService = siteService;
        }

        public override async Task<IDisplayResult> DisplayAsync(ContentItem model, BuildDisplayContext context)
        {
            if (await _deploymentPlanService.DoesUserHaveExportPermissionAsync())
            {
                var siteSettings = await _siteService.GetSiteSettingsAsync();
                var exportContentToDeploymentTargetSettings = siteSettings.As<ExportContentToDeploymentTargetSettings>();
                if (exportContentToDeploymentTargetSettings.ExportContentToDeploymentTargetPlanId != 0)
                {
                    return Combine(
                        Dynamic("ExportContentToDeploymentTarget_Modal__ActionDeploymentTarget").Location("SummaryAdmin", "ActionsMenu:30"),
                        Shape("ExportContentToDeploymentTarget_SummaryAdmin__Button__Actions", new ContentItemViewModel(model)).Location("SummaryAdmin", "ActionsMenu:40")
                    );
                }
            }

            return null;
        }
    }
}
