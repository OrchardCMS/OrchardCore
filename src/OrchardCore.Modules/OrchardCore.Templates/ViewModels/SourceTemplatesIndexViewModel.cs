using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Admin;
using OrchardCore.Templates.Models;
using OrchardCore.Templates.Services;

namespace OrchardCore.Templates.ViewModels;

public class SourceTemplatesIndexViewModel
{
    /// <summary>
    /// The template source the page lists.
    /// </summary>
    [BindNever]
    public ITemplateSource Source { get; set; }

    /// <summary>
    /// The data context for rendering the templates, the toolbar and the pager in the configured layout.
    /// </summary>
    [BindNever]
    public AdminListContext List { get; set; }

    public SourceTemplatesOptions Options { get; set; } = new();
}

public class SourceTemplatesOptions
{
    public bool AdminTemplates { get; set; }
    public string Search { get; set; }
    public SourceTemplatesFilter Filter { get; set; }
    public SourceTemplatesBulkAction BulkAction { get; set; }

    [BindNever]
    public List<SelectListItem> BulkActions { get; set; }

    [BindNever]
    public List<SelectListItem> Filters { get; set; }
}

/// <summary>
/// Filters the templates of a source by the state of their local overrides.
/// </summary>
public enum SourceTemplatesFilter
{
    /// <summary>All the templates.</summary>
    All,

    /// <summary>The templates without a local override.</summary>
    NotOverridden,

    /// <summary>The templates with a local override.</summary>
    Overridden,

    /// <summary>The cloned templates that were edited since.</summary>
    Modified,

    /// <summary>The cloned templates whose source changed since, e.g. after the theme was updated.</summary>
    SourceChanged,
}

public enum SourceTemplatesBulkAction
{
    None,

    /// <summary>Clones the Liquid templates without a local override.</summary>
    Clone,

    /// <summary>Replaces the local overrides of the Liquid templates with the current source.</summary>
    Reset,

    /// <summary>Deletes the local overrides.</summary>
    Remove,
}
