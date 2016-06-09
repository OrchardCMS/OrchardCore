using Microsoft.Extensions.Localization;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services
{
    public abstract class RecipeBuilderStep : IRecipeBuilderStep
    {
        public abstract string Name { get; }
        public abstract LocalizedString DisplayName { get; }
        public abstract LocalizedString Description { get; }
        public virtual int Priority { get { return 0; } }
        public virtual int Position { get { return 0; } }
        public virtual bool IsVisible { get { return true; } }

        protected virtual string Prefix
        {
            get { return GetType().Name; }
        }

        public virtual dynamic BuildEditor(dynamic shapeFactory)
        {
            return null;
        }

        public virtual dynamic UpdateEditor(dynamic shapeFactory)
        {
            return null;
        }

        public virtual void Configure(RecipeBuilderStepConfigurationContext context)
        {
        }

        public virtual void ConfigureDefault()
        {
        }

        public virtual void Build(BuildContext context) { }
    }
}