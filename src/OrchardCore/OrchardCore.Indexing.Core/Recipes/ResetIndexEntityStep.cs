using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using OrchardCore.Indexing.Core.Deployments;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Indexing.Core.Recipes;

public sealed class ResetIndexEntityStep : NamedRecipeStepHandler
{
    public const string Key = "ResetIndexing";

    public ResetIndexEntityStep()
        : base(Key)
    {
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<IndexEntityDeploymentStep>();

        if (model == null)
        {
            return;
        }

        if (!model.IncludeAll && (model.Indexes == null || model.Indexes.Length == 0))
        {
            return;
        }

        await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("reset-indexing", async scope =>
        {
            var indexEntityManager = scope.ServiceProvider.GetService<IIndexEntityManager>();

            var indexes = model.IncludeAll
            ? await indexEntityManager.GetAllAsync()
            : (await indexEntityManager.GetAllAsync()).Where(x => model.Indexes.Contains(x.IndexName, StringComparer.OrdinalIgnoreCase));

            foreach (var index in indexes)
            {
                await indexEntityManager.ResetAsync(index);
                await indexEntityManager.UpdateAsync(index);
                await indexEntityManager.SynchronizeAsync(index);
            }
        });
    }
}
