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
    /// The scope of the source the page lists, e.g. a theme, if the source has scopes.
    /// </summary>
    [BindNever]
    public TemplateSourceScope Scope { get; set; }

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

    /// <summary>
    /// The scope of the source to list, e.g. a theme, or <see langword="null"/> for the one in use.
    /// </summary>
    public string Scope { get; set; }
    public string Search { get; set; }
    public SourceTemplatesFilter Filter { get; set; }

    /// <summary>
    /// Filters the templates by what they render, for the sources telling it, the front end on the templates
    /// and the admin on the admin templates by default.
    /// </summary>
    public SourceTemplatesArea? Area { get; set; }

    /// <summary>
    /// Filters the templates by their language, Liquid or Razor.
    /// </summary>
    public SourceTemplatesLanguage Language { get; set; }
    public SourceTemplatesBulkAction BulkAction { get; set; }

    [BindNever]
    public List<SelectListItem> BulkActions { get; set; }

    [BindNever]
    public List<SelectListItem> Filters { get; set; }

    [BindNever]
    public List<SelectListItem> Scopes { get; set; }

    [BindNever]
    public List<SelectListItem> Areas { get; set; }

    [BindNever]
    public List<SelectListItem> Languages { get; set; }
}

/// <summary>
/// Filters the templates of a source by their language.
/// </summary>
public enum SourceTemplatesLanguage
{
    /// <summary>All the templates.</summary>
    All,

    /// <summary>The Liquid templates, which can be cloned.</summary>
    Liquid,

    /// <summary>The Razor templates, which can only be viewed.</summary>
    Razor,
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

/// <summary>
/// Filters the templates of a source by what they render, as guessed from their names.
/// </summary>
public enum SourceTemplatesArea
{
    /// <summary>All the templates.</summary>
    All,

    /// <summary>The templates rendering the front end, e.g. the display of a content part.</summary>
    FrontEnd,

    /// <summary>The templates rendering the admin, e.g. an editor.</summary>
    Admin,
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
