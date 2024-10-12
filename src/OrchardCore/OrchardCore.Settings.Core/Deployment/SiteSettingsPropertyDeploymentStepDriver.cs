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

    public SiteSettingsPropertyDeploymentStepDriver(string title, string description) : base(null)
    {
        _title = title;
        _description = description;
    }

    public override IDisplayResult Edit(SiteSettingsPropertyDeploymentStep<TModel> step, Action<SiteSettingsPropertyDeploymentStepViewModel> intializeAction)
    {
        return base.Edit(step, model =>
        {
            model.Title = _title;
            model.Description = _description;
        });
    }
}
