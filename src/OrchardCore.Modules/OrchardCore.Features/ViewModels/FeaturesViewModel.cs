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

    /// <summary>
    /// The layouts a user can switch the list to, or <see langword="null"/> when the site keeps the choice to
    /// itself. The page carries it in the bar holding its filters: its categories share one layout, so a
    /// selector on each of them would offer the same thing twenty times over.
    /// </summary>
    public IShape LayoutSelector { get; set; }
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
