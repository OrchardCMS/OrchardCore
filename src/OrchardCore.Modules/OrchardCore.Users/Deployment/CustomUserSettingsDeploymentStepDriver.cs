using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Deployment;

public sealed class CustomUserSettingsDeploymentStepDriver
    : DeploymentStepFieldsDriverBase<CustomUserSettingsDeploymentStep, CustomUserSettingsDeploymentStepViewModel>
{
    private readonly CustomUserSettingsService _customUserSettingsService;

    public CustomUserSettingsDeploymentStepDriver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _customUserSettingsService = serviceProvider.GetService<CustomUserSettingsService>();
    }

    public override IDisplayResult Edit(CustomUserSettingsDeploymentStep step, Action<CustomUserSettingsDeploymentStepViewModel> intializeAction)
    {
        return base.Edit(step, async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.SettingsTypeNames = step.SettingsTypeNames;
            model.AllSettingsTypeNames = (await _customUserSettingsService.GetAllSettingsTypeNamesAsync()).ToArray();
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(CustomUserSettingsDeploymentStep step, UpdateEditorContext context)
    {
        step.SettingsTypeNames = [];
        await context.Updater.TryUpdateModelAsync(step, Prefix);

        if (step.IncludeAll)
        {
            step.SettingsTypeNames = [];
        }

        return Edit(step, context);
    }
}
