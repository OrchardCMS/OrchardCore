using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.AzureAI.Deployment;
using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Search.AzureAI.ViewModels;

namespace OrchardCore.Search.AzureAI.Drivers;

public sealed class AzureAISearchIndexRebuildDeploymentStepDriver
    : DeploymentStepFieldsDriverBase<AzureAISearchIndexRebuildDeploymentStep, AzureAISearchIndexRebuildDeploymentStepViewModel>
{
    private readonly AzureAISearchIndexSettingsService _indexSettingsService;

    public AzureAISearchIndexRebuildDeploymentStepDriver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _indexSettingsService = serviceProvider.GetService<AzureAISearchIndexSettingsService>();
    }

    public override IDisplayResult Edit(AzureAISearchIndexRebuildDeploymentStep step, Action<AzureAISearchIndexRebuildDeploymentStepViewModel> intializeAction)
    {
        return base.Edit(step, async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.IndexNames = step.Indices;
            model.AllIndexNames = (await _indexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(AzureAISearchIndexRebuildDeploymentStep step, UpdateEditorContext context)
    {
        step.Indices = [];

        await context.Updater.TryUpdateModelAsync(step, Prefix,
            p => p.IncludeAll,
            p => p.Indices);

        if (step.IncludeAll)
        {
            // Clear index names if the user select include all.
            step.Indices = [];
        }

        return Edit(step, context);
    }
}
