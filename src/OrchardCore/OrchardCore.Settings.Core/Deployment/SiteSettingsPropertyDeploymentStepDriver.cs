using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings.ViewModels;

namespace OrchardCore.Settings.Deployment;

public class SiteSettingsPropertyDeploymentStepDriver<TModel>
    : DeploymentStepFieldsDriverBase<SiteSettingsPropertyDeploymentStep<TModel>, SiteSettingsPropertyDeploymentStepViewModel> where TModel : class, new()
{
    private readonly string _title;
    private readonly string _description;

    public SiteSettingsPropertyDeploymentStepDriver(string title, string description, IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        _title = title;
        _description = description;
    }

    public override Task<IDisplayResult> DisplayAsync(SiteSettingsPropertyDeploymentStep<TModel> step, BuildDisplayContext context)
    {
        return CombineAsync(
            Initialize<SiteSettingsPropertyDeploymentStepViewModel>(DisplaySummaryShape, m => BuildViewModel(m))
                    .Location("Summary", "Content"),
                Initialize<SiteSettingsPropertyDeploymentStepViewModel>(DisplayThumbnailShape, m => BuildViewModel(m))
                    .Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(SiteSettingsPropertyDeploymentStep<TModel> step, BuildEditorContext context)
    {
        return Initialize<SiteSettingsPropertyDeploymentStepViewModel>(EditShape, BuildViewModel)
            .Location("Content");
    }

    private void BuildViewModel(SiteSettingsPropertyDeploymentStepViewModel model)
    {
        model.Title = _title;
        model.Description = _description;
    }
}
