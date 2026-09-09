using OrchardCore.DisplayManagement;
using OrchardCore.Features.Models;

namespace OrchardCore.Features.ViewModels;

public class FeaturesViewModel
{
    public string Name { get; set; }

    /// <summary>
    /// True when the current tenant is the Default one, and is executing on behalf of other tenant. Otherwise false.
    /// </summary>
    public bool IsProxy { get; set; }

    public IEnumerable<ModuleFeature> Features { get; set; }

    /// <summary>
    /// The features grouped by category, one <c>AdminList</c> shape per category.
    /// </summary>
    public IList<FeatureGroupViewModel> Groups { get; set; } = [];
}

public class FeatureGroupViewModel
{
    /// <summary>
    /// The category the features of this group belong to.
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// The <c>AdminList</c> shape rendering the features of this category with the configured layout.
    /// </summary>
    public IShape List { get; set; }
}
