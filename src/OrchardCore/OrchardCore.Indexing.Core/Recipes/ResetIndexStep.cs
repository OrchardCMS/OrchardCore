using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Indexing.Core.Deployments;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Indexing.Core.Recipes;

public sealed class ResetIndexStep : NamedRecipeStepHandler
{
    public const string Key = "ResetIndex";

    public ResetIndexStep()
        : base(Key)
    {
    }

    protected override Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<IndexProfileDeploymentStep>();

        if (model == null)
        {
            return Task.CompletedTask;
        }

        if (!model.IncludeAll && (model.IndexNames == null || model.IndexNames.Length == 0))
        {
            return Task.CompletedTask;
        }

        ShellScope.ExecuteInBackgroundAfterRequestAsync(async scope =>
        {
            var indexProfileManager = scope.ServiceProvider.GetService<IIndexProfileManager>();

            var indexes = model.IncludeAll
            ? await indexProfileManager.GetAllAsync()
            : (await indexProfileManager.GetAllAsync()).Where(x => model.IndexNames.Contains(x.Name, StringComparer.OrdinalIgnoreCase));

            foreach (var index in indexes)
            {
                await indexProfileManager.ResetAsync(index);
                await indexProfileManager.UpdateAsync(index);
                await indexProfileManager.SynchronizeAsync(index);
            }
        }, "reset-indexes");

        return Task.CompletedTask;
    }
}
