using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Features.Models;

/// <summary>
/// One row of the features admin list. Its name is the shape type of the rows built by
/// <see cref="Drivers.FeatureEntryDisplayDriver"/>, so themes override them with
/// <c>FeatureEntry-SummaryAdmin.cshtml</c>.
/// </summary>
public class FeatureEntry
{
    /// <summary>
    /// The feature this row displays.
    /// </summary>
    public ModuleFeature Feature { get; set; }

    /// <summary>
    /// The tenant the feature belongs to, when the features of another tenant are managed from here.
    /// </summary>
    public string Tenant { get; set; }

    /// <summary>
    /// Whether the feature is enabled and cannot be disabled from here.
    /// </summary>
    public bool IsAlwaysEnabled { get; set; }

    /// <summary>
    /// Whether the feature can be enabled from here.
    /// </summary>
    public bool CanEnable { get; set; }

    /// <summary>
    /// Whether the feature can be disabled from here.
    /// </summary>
    public bool CanDisable { get; set; }

    /// <summary>
    /// Whether the row has a checkbox, i.e. the feature can be enabled or disabled by a bulk action.
    /// </summary>
    public bool IsSelectable
        => (CanEnable && !Feature.IsEnabled) || (CanDisable && Feature.IsEnabled);

    /// <summary>
    /// The features this feature declares as its dependencies.
    /// </summary>
    public IList<IFeatureInfo> DirectDependencies { get; set; } = [];

    /// <summary>
    /// The features this feature depends on through its dependencies.
    /// </summary>
    public IList<IFeatureInfo> IndirectDependencies { get; set; } = [];

    /// <summary>
    /// The dependencies that are not available, which is why the feature cannot be enabled.
    /// </summary>
    public IList<IFeatureInfo> MissingDependencies { get; set; } = [];
}
