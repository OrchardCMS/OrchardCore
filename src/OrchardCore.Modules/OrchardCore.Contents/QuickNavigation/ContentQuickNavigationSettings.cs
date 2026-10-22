using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Contents.QuickNavigation;

/// <summary>
/// Controls the recent content items contributed by the Content Quick Navigation feature.
/// </summary>
public class ContentQuickNavigationSettings
{
    /// <summary>
    /// Gets or sets the maximum number of recently modified, latest content items to consider.
    /// The limit is applied before checking the current user's edit permissions.
    /// </summary>
    [DefaultValue(50)]
    [Range(1, int.MaxValue, ErrorMessage = "The number of recent content items must be at least 1.")]
    public int MaxItems { get; set; } = 50;
}
