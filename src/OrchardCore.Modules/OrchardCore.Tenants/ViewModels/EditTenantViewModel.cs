using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Tenants.ViewModels
{
    public class EditTenantViewModel : TenantViewModel
    {
        public string Category { get; set; }

        public string Name { get; set; }

        public string DatabaseProvider { get; set; }

        public string RequestUrlPrefix { get; set; }

        public string RequestUrlHost { get; set; }

        public string ConnectionString { get; set; }

        public string TablePrefix { get; set; }

        public string RecipeName { get; set; }
        
        public string[] FeatureProfiles { get; set; }
        
        public List<SelectListItem> FeatureProfileItems { get; set; }
        
        public List<SelectListItem> FeatureProfiles { get; set; }
        
        public IEnumerable<RecipeDescriptor> Recipes { get; set; }

        public bool CanEditDatabasePresets { get; set; }

        public bool DatabaseConfigurationPreset { get; set; }
    }
}
