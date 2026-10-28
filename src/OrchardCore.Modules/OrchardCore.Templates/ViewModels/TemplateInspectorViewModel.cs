using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.ContentManagement;
using OrchardCore.Templates.Models;

namespace OrchardCore.Templates.ViewModels;

/// <summary>
/// The Inspect Content page: the content item to inspect, and the shapes it renders with their templates and alternates.
/// </summary>
public class TemplateInspectorViewModel
{
    /// <summary>
    /// Whether the content item is inspected as the admin renders it, rather than the front end.
    /// </summary>
    public bool AdminTemplates { get; set; }

    public string ContentItemId { get; set; }

    public string DisplayType { get; set; }

    /// <summary>
    /// The recent content items to pick from, by content type.
    /// </summary>
    public IReadOnlyList<SelectListItem> ContentItems { get; set; } = [];

    public IReadOnlyList<SelectListItem> DisplayTypes { get; set; } = [];

    /// <summary>
    /// The inspected content item, or <see langword="null"/> when none is picked.
    /// </summary>
    public ContentItem ContentItem { get; set; }

    /// <summary>
    /// The shapes the content item renders, each one before the shapes it renders.
    /// </summary>
    public IReadOnlyList<InspectedShape> Shapes { get; set; } = [];

    /// <summary>
    /// The names of the local templates, to tell which alternates are overridden.
    /// </summary>
    public ISet<string> LocalTemplates { get; set; } = new HashSet<string>();

    /// <summary>
    /// The templates of the sources, e.g. of the active theme, by the shape type or alternate they are bound to,
    /// to view or clone them.
    /// </summary>
    public IReadOnlyDictionary<string, InspectedSourceTemplate> SourceTemplates { get; set; } = new Dictionary<string, InspectedSourceTemplate>();

    /// <summary>
    /// Why the content item could not be rendered, if it could not.
    /// </summary>
    public string Error { get; set; }
}
