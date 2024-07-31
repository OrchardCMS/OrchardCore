using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings.ViewModels;

namespace OrchardCore.Settings.Deployment
{
    public class SiteSettingsPropertyDeploymentStepDriver<TModel> : DisplayDriver<DeploymentStep, SiteSettingsPropertyDeploymentStep<TModel>> where TModel : class, new()
    {
        private readonly string _title;
        private readonly string _description;

        public SiteSettingsPropertyDeploymentStepDriver(string title, string description)
        {
            _title = title;
            _description = description;
        }

        public override Task<IDisplayResult> DisplayAsync(SiteSettingsPropertyDeploymentStep<TModel> step, BuildDisplayContext context)
        {
            return CombineAsync(
                    Initialize<SiteSettingsPropertyDeploymentStepViewModel>("SiteSettingsPropertyDeploymentStep_Fields_Summary", m => BuildViewModel(m))
                        .Location("Summary", "Content"),
                    Initialize<SiteSettingsPropertyDeploymentStepViewModel>("SiteSettingsPropertyDeploymentStep_Fields_Thumbnail", m => BuildViewModel(m))
                        .Location("Thumbnail", "Content")
                );
        }

        public override Task<IDisplayResult> EditAsync(SiteSettingsPropertyDeploymentStep<TModel> step, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                Initialize<SiteSettingsPropertyDeploymentStepViewModel>("SiteSettingsPropertyDeploymentStep_Fields_Edit", m => BuildViewModel(m))
                .Location("Content"));
        }

        private void BuildViewModel(SiteSettingsPropertyDeploymentStepViewModel model)
        {
            model.Title = _title;
            model.Description = _description;
        }
    }
}
