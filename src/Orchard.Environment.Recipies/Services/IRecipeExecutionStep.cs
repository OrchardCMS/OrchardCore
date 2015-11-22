using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Models;
using Orchard.ContentManagement;

namespace Orchard.Environment.Recipes.Services
{
    public interface IRecipeExecutionStep : IDependency
    {
        string Name { get; }
        IEnumerable<string> Names { get; }
        LocalizedString DisplayName { get; }
        LocalizedString Description { get; }
        dynamic BuildEditor(dynamic shapeFactory);
        dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater);
        void Configure(RecipeExecutionStepConfigurationContext context);
        void UpdateStep(UpdateRecipeExecutionStepContext context);
        void Execute(RecipeExecutionContext context);
    }
}