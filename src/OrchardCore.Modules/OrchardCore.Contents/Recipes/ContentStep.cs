using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Contents.Recipes;

/// <summary>
/// This recipe step creates a set of content items.
/// </summary>
/// <remarks>
/// This class is obsolete. Implement <see cref="IRecipeDeploymentStep"/> instead.
/// </remarks>
[Obsolete($"Implement {nameof(IRecipeDeploymentStep)} instead. This class will be removed in a future version.", false)]
#pragma warning disable CS0618 // Type or member is obsolete - required for backwards compatibility
public sealed class ContentStep : NamedRecipeStepHandler
#pragma warning restore CS0618
{
    public ContentStep()
        : base("Content")
    {
    }

    protected override Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<ContentStepModel>();
        var contentItems = model.Data.ToObject<ContentItem[]>();

        // If the shell is activated there is no migration in progress.
        if (ShellScope.Context.IsActivated)
        {
            var contentManager = ShellScope.Services.GetRequiredService<IContentManager>();

            return contentManager.ImportAsync(contentItems);
        }

        // Otherwise, the import of content items is deferred after all migrations are completed,
        // this prevents e.g. a content handler to trigger a workflow before workflows migrations.
        ShellScope.AddDeferredTask(scope =>
        {
            var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();

            return contentManager.ImportAsync(contentItems);
        });

        return Task.CompletedTask;
    }
}

/// <remarks>
/// This class is obsolete. Use your own model class instead.
/// </remarks>
[Obsolete("Use your own model class instead. This class will be removed in a future version.", false)]
public sealed class ContentStepModel
{
    public JsonArray Data { get; set; }
}
