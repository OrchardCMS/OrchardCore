using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Models;

namespace Orchard.Environment.Recipes.Services {
    public interface IRecipeExecutionStep : IDependency {
        string Name { get; }
        IEnumerable<string> Names { get; }
        LocalizedString DisplayName { get; }
        LocalizedString Description { get; }
        void Configure(RecipeExecutionStepConfigurationContext context);
        void UpdateStep(UpdateRecipeExecutionStepContext context);
        void Execute(RecipeExecutionContext context);
    }
}