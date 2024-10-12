using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.AzureAI.Deployment;
using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Search.AzureAI.ViewModels;

namespace OrchardCore.Search.AzureAI.Drivers;

public sealed class AzureAISearchIndexDeploymentStepDriver
    : DeploymentStepFieldsDriverBase<AzureAISearchIndexDeploymentStep>
{
    private readonly AzureAISearchIndexSettingsService _indexSettingsService;

    public AzureAISearchIndexDeploymentStepDriver(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        _indexSettingsService = serviceProvider.GetService<AzureAISearchIndexSettingsService>();
    }

    public override IDisplayResult Edit(AzureAISearchIndexDeploymentStep step, BuildEditorContext context)
        => Initialize<AzureAISearchIndexDeploymentStepViewModel>("AzureAISearchIndexDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.IndexNames;
            model.AllIndexNames = (await _indexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();
        }).Location("Content");


    public override async Task<IDisplayResult> UpdateAsync(AzureAISearchIndexDeploymentStep step, UpdateEditorContext context)
    {
        step.IndexNames = [];

        await context.Updater.TryUpdateModelAsync(step, Prefix,
            p => p.IncludeAll,
            p => p.IndexNames);

        if (step.IncludeAll)
        {
            // Clear index names if the user select include all.
            step.IndexNames = [];
        }

        return Edit(step, context);
    }
}
