using System.Text.Json.Nodes;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.ContentTypes.RecipeSteps;

/// <summary>
/// This recipe step deletes content definition records.
/// </summary>
public sealed class DeleteContentDefinitionStep : NamedRecipeStepHandler
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public DeleteContentDefinitionStep(IContentDefinitionManager contentDefinitionManager)
        : base("DeleteContentDefinition")
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var step = context.Step.ToObject<DeleteContentDefinitionStepModel>();

        foreach (var contentType in step.ContentTypes)
        {
            // The content definition manager tests existence before trying to delete.
            await _contentDefinitionManager.DeleteTypeDefinitionAsync(contentType);
        }

        foreach (var contentPart in step.ContentParts)
        {
            // The content definition manager tests existence before trying to delete.
            await _contentDefinitionManager.DeletePartDefinitionAsync(contentPart);
        }
    }

    private sealed class DeleteContentDefinitionStepModel
    {
        public string[] ContentTypes { get; set; } = [];
        public string[] ContentParts { get; set; } = [];
    }
}
