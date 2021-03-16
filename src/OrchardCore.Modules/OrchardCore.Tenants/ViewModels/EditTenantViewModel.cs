using System.Collections.Generic;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Tenants.ViewModels
{
    public class EditTenantViewModel
    {
        public string Description { get; set; }

        public string Name { get; set; }

        public string DatabaseProvider { get; set; }

        public string RequestUrlPrefix { get; set; }

        public string RequestUrlHost { get; set; }

        public string ConnectionString { get; set; }

        public string TablePrefix { get; set; }

        public string RecipeName { get; set; }

        public IEnumerable<RecipeDescriptor> Recipes { get; set; }

        public bool CanSetDatabasePresets { get; set; }
    }
}
