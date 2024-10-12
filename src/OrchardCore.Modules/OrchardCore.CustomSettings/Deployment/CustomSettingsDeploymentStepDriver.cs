using Microsoft.Extensions.DependencyInjection;
using OrchardCore.CustomSettings.Services;
using OrchardCore.CustomSettings.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.CustomSettings.Deployment;

public sealed class CustomSettingsDeploymentStepDriver
    : DeploymentStepFieldsDriverBase<CustomSettingsDeploymentStep, CustomSettingsDeploymentStepViewModel>
{
    private readonly CustomSettingsService _customSettingsService;

    public CustomSettingsDeploymentStepDriver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _customSettingsService = serviceProvider.GetService<CustomSettingsService>();
    }

    public override IDisplayResult Edit(CustomSettingsDeploymentStep step, Action<CustomSettingsDeploymentStepViewModel> intializeAction)
    {
        return base.Edit(step, async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.SettingsTypeNames = step.SettingsTypeNames;
            model.AllSettingsTypeNames = (await _customSettingsService.GetAllSettingsTypeNamesAsync()).ToArray();
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(CustomSettingsDeploymentStep step, UpdateEditorContext context)
    {
        step.SettingsTypeNames = [];

        await context.Updater.TryUpdateModelAsync(step,
                                          Prefix,
                                          x => x.SettingsTypeNames,
                                          x => x.IncludeAll);

        // Don't have the selected option if include all.
        if (step.IncludeAll)
        {
            step.SettingsTypeNames = [];
        }

        return Edit(step, context);
    }
}
