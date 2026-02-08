using System.Text.Json.Nodes;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Templates.Models;
using OrchardCore.Templates.Services;

namespace OrchardCore.Templates.Recipes;

/// <summary>
/// This recipe step creates a set of templates.
/// </summary>
[Obsolete("Implement IRecipeDeploymentStep instead. This class will be removed in a future version.", false)]
#pragma warning disable CS0618 // Type or member is obsolete
public sealed class TemplateStep : NamedRecipeStepHandler
#pragma warning restore CS0618
{
    private readonly TemplatesManager _templatesManager;

    public TemplateStep(TemplatesManager templatesManager)
        : base("Templates")
    {
        _templatesManager = templatesManager;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        if (context.Step.TryGetPropertyValue("Templates", out var jsonNode) && jsonNode is JsonObject templates)
        {
            foreach (var property in templates)
            {
                var name = property.Key;
                var value = property.Value.ToObject<Template>();

                await _templatesManager.UpdateTemplateAsync(name, value);
            }
        }
    }
}
