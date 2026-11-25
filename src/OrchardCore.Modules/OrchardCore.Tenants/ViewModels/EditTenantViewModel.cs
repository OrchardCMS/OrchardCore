using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Tenants.ViewModels;

public class EditTenantViewModel : TenantViewModel
{
    public List<SelectListItem> FeatureProfilesItems { get; set; }

    public IEnumerable<RecipeDescriptor> Recipes { get; set; }

    public bool CanEditDatabasePresets { get; set; }

    public bool DatabaseConfigurationPreset { get; set; }

    [BindNever]
    public bool DatabaseProviderPreset { get; set; }

    [BindNever]
    public bool RequireTablePrefix { get; set; }

    [BindNever]
    public bool HasTablePrefixPattern { get; set; }

    [BindNever]
    public string ResolvedTablePrefix { get; set; }

    [BindNever]
    public bool HasSchemaPattern { get; set; }

    [BindNever]
    public string ResolvedSchema { get; set; }

    /// <summary>
    /// Gets or sets the editor of the tenant, built by the display drivers of <see cref="EditTenantViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}
