using Orchard.Environment.Recipes.Models;
using Microsoft.Framework.Localization;
using Orchard.DependencyInjection;

namespace Orchard.Environment.Recipes.Services {
    public interface IRecipeBuilderStep : IDependency {
        string Name { get; }
        LocalizedString DisplayName { get; }
        LocalizedString Description { get; }

        /// <summary>
        /// The order in which this builder should execute.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// The order in which this builder should be displayed.
        /// </summary>
        int Position { get; }
        bool IsVisible { get; }
        
        void Configure(RecipeBuilderStepConfigurationContext configurationElement);
        void ConfigureDefault();
        void Build(BuildContext context);
    }
}