using System.Collections.Generic;

namespace OrchardCore.Recipes.Models
{
    public class RecipeEnvironmentFeature
    {
        public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();
    }
}
