using Microsoft.Extensions.Localization;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes.RecipeSteps;

/// <summary>
/// Reports that the commands of a <see cref="CommandStep"/> aren't checked, as they can do anything, including
/// enabling the features that the next steps need.
/// </summary>
public sealed class CommandStepValidator : IRecipeStepValidator
{
    internal readonly IStringLocalizer S;

    public CommandStepValidator(IStringLocalizer<CommandStepValidator> stringLocalizer)
    {
        S = stringLocalizer;
    }

    /// <inheritdoc />
    public string StepName => "Command";

    /// <inheritdoc />
    public Task ValidateAsync(RecipeValidationContext context)
    {
        context.FeaturesMayChange = true;
        context.AddInfo("commands-not-validated", S["The commands of this step aren't validated."]);

        return Task.CompletedTask;
    }
}
