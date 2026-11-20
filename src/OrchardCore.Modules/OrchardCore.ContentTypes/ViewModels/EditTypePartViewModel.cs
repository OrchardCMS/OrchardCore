using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentTypes.ViewModels;

public class EditTypePartViewModel
{
    /// <summary>
    /// The technical name of the part.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The display name of the part.
    /// </summary>
    public string DisplayName { get; set; }

    public string Description { get; set; }

    [BindNever]
    public ContentTypePartDefinition TypePartDefinition { get; set; }

    [BindNever]
    public dynamic Shape { get; set; }

    /// <summary>
    /// The editor name of the part.
    /// </summary>
    public string Editor { get; set; }

    /// <summary>
    /// The display mode of the part.
    /// </summary>
    public string DisplayMode { get; set; }

    /// <summary>
    /// Gets or sets the names of the shapes rendering the options of the editor select, named <c>{PartName}_Option__{Editor}</c>.
    /// </summary>
    [BindNever]
    public IEnumerable<string> EditorShapes { get; set; } = [];

    /// <summary>
    /// Gets or sets the names of the shapes rendering the options of the display mode select, named <c>{PartName}_DisplayOption__{DisplayMode}</c>.
    /// </summary>
    [BindNever]
    public IEnumerable<string> DisplayShapes { get; set; } = [];
}
