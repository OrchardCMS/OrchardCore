using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings.ViewModels;

namespace OrchardCore.Settings.Deployment
{
    public class SiteSettingsDeploymentStepDriver<TModel> : DisplayDriver<DeploymentStep, SiteSettingsDeploymentStep<TModel>> where TModel : class
    {
        private readonly string _title;
        private readonly string _description;

        public SiteSettingsDeploymentStepDriver(string title, string description)
        {
            _title = title;
            _description = description;
        }

        public override IDisplayResult Display(SiteSettingsDeploymentStep<TModel> step)
        {
            return
                Combine(
                    Initialize<SiteSettingsDeploymentStepViewModel>("SiteSettingsDeploymentStep_Fields_Summary", model =>
                    {
                        model.Title = _title;
                        model.Description = _description;
                    }).Location("Summary", "Content"),
                    Initialize<SiteSettingsDeploymentStepViewModel>("SiteSettingsDeploymentStep_Fields_Thumbnail", model =>
                    {
                        model.Title = _title;
                        model.Description = _description;
                    }).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(SiteSettingsDeploymentStep<TModel> step)
        {
            return Initialize<SiteSettingsDeploymentStepViewModel>("SiteSettingsDeploymentStep_Fields_Edit", model =>
            {
                model.Title = _title;
                model.Description = _description;
            }).Location("Content");
        }
    }
}
