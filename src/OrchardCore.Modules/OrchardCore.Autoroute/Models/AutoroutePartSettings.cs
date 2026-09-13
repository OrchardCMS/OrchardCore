using System.ComponentModel;

namespace OrchardCore.Autoroute.Models;

public class AutoroutePartSettings
{
    /// <summary>
    /// Gets or sets whether a user can define a custom path.
    /// </summary>
    [Description("Whether editors and remote clients can define a custom path.")]
    public bool AllowCustomPath { get; set; }

    /// <summary>
    /// The pattern used to build the Path.
    /// </summary>
    [DefaultValue("{{ ContentItem.DisplayText | slugify }}")]
    [Description("The Liquid pattern used to generate the content item path.")]
    public string Pattern { get; set; } = "{{ ContentItem.DisplayText | slugify }}";

    /// <summary>
    /// Whether to display an option to set the content item as the homepage.
    /// </summary>
    [Description("Whether the editor displays the privileged option for selecting the content item as the homepage.")]
    public bool ShowHomepageOption { get; set; }

    /// <summary>
    /// Whether a user can request a new path if some data has changed.
    /// </summary>
    [Description("Whether editors and remote clients can request path regeneration after content changes.")]
    public bool AllowUpdatePath { get; set; }

    /// <summary>
    /// Whether a user is allowed to disable autoute generation to content items of this type.
    /// </summary>
    [Description("Whether editors and remote clients can disable autoroute generation.")]
    public bool AllowDisabled { get; set; }

    /// <summary>
    /// Whether to allow routing of contained content items.
    /// </summary>
    [Description("Whether routing can be enabled for contained content items.")]
    public bool AllowRouteContainedItems { get; set; }

    /// <summary>
    /// Whether this part is managing contained item routes.
    /// </summary>
    [Description("Whether this attachment manages routes for contained content items.")]
    public bool ManageContainedItemRoutes { get; set; }

    /// <summary>
    /// Whether to allow routing of contained content items to absolute paths.
    /// </summary>
    [Description("Whether contained content items can use absolute paths.")]
    public bool AllowAbsolutePath { get; set; }
}
