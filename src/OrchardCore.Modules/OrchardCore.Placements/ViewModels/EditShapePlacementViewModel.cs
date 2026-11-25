using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Placements.ViewModels;

public class EditShapePlacementViewModel
{
    public bool Creating { get; set; }

    public string ShapeType { get; set; }

    public string Nodes { get; set; }

    /// <summary>
    /// Gets or sets the editor of the placement rules, built by the display drivers of <see cref="EditShapePlacementViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}
