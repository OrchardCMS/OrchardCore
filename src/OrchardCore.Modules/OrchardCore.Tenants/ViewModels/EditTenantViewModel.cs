using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Tenants.ViewModels
{
    public class EditTenantViewModel : TenantViewModel
    {
        public List<SelectListItem> FeatureProfilesItems { get; set; }

        public IEnumerable<RecipeDescriptor> Recipes { get; set; }

        public bool CanEditDatabasePresets { get; set; }

        public bool DatabaseConfigurationPreset { get; set; }
    }
}
