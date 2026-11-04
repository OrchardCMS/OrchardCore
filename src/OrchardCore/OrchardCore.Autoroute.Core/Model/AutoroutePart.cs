using System.ComponentModel;
using System.Text.Json.Serialization;
using OrchardCore.ContentManagement;

namespace OrchardCore.Autoroute.Models;

public class AutoroutePart : ContentPart
{
    public const int MaxPathLength = 1024;

    public static readonly char[] InvalidCharactersForPath = ":?#[]@!$&'()*+,.;=<>\\|%".ToCharArray();

    [Description("The URL path for the content item, relative to the site's base URL.")]
    public string Path { get; set; }

    /// <summary>
    /// Whether to make the content item the homepage once it's published.
    /// </summary>
    [JsonIgnore]
    public bool SetHomepage { get; set; }

    /// <summary>
    /// Whether to disable autoroute generation for this content item.
    /// When disabled the path is no longer validated, or generated.
    /// </summary>
    [Description("Whether automatic route generation is disabled for the content item.")]
    public bool Disabled { get; set; }

    /// <summary>
    /// Whether to route content items contained within the content item.
    /// </summary>
    [Description("Whether contained content items receive routes beneath this content item's path.")]
    public bool RouteContainedItems { get; set; }

    /// <summary>
    /// When this content item is contained within another is the route relative to the container route.
    /// </summary>
    [Description("Whether contained content item paths are absolute instead of relative to the container path.")]
    public bool Absolute { get; set; }
}
