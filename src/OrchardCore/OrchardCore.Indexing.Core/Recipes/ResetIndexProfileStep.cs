using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using OrchardCore.Indexing.Core.Deployments;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Indexing.Core.Recipes;

public sealed class ResetIndexProfileStep : NamedRecipeStepHandler
{
    public const string Key = "ResetIndexProfile";

    public ResetIndexProfileStep()
        : base(Key)
    {
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<IndexProfileDeploymentStep>();

        if (model == null)
        {
            return;
        }

        if (!model.IncludeAll && (model.IndexIds == null || model.IndexIds.Length == 0))
        {
            return;
        }

        await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("reset-index-profile", async scope =>
        {
            var indexProfileManager = scope.ServiceProvider.GetService<IIndexProfileManager>();

            var indexes = model.IncludeAll
            ? await indexProfileManager.GetAllAsync()
            : (await indexProfileManager.GetAllAsync()).Where(x => model.IndexIds.Contains(x.Id, StringComparer.OrdinalIgnoreCase) || model.IndexIds.Contains(x.Name, StringComparer.OrdinalIgnoreCase));

            foreach (var index in indexes)
            {
                await indexProfileManager.ResetAsync(index);
                await indexProfileManager.UpdateAsync(index);
                await indexProfileManager.SynchronizeAsync(index);
            }
        });
    }
}
