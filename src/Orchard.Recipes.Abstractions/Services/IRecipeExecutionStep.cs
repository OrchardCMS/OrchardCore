using Microsoft.Extensions.Localization;
using Orchard.Recipes.Models;
using System.Collections.Generic;

namespace Orchard.Recipes.Services
{
    public interface IRecipeExecutionStep
    {
        string Name { get; }
        IEnumerable<string> Names { get; }
        LocalizedString DisplayName { get; }
        LocalizedString Description { get; }
        dynamic BuildEditor(dynamic shapeFactory);
        dynamic UpdateEditor(dynamic shapeFactory);
        void Configure(RecipeExecutionStepConfigurationContext context);
        void UpdateStep(UpdateRecipeExecutionStepContext context);
        void Execute(RecipeExecutionContext context);
    }
}