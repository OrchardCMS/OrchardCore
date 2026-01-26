using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Tenants.ViewModels;

public class EditTenantViewModel : TenantViewModel
{
    public List<SelectListItem> FeatureProfilesItems { get; set; }

    public IEnumerable<IRecipeDescriptor> Recipes { get; set; }

    public bool CanEditDatabasePresets { get; set; }

    public bool DatabaseConfigurationPreset { get; set; }
}
