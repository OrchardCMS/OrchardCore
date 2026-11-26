using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Layers.ViewModels;

public class LayerEditViewModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public dynamic LayerRule { get; set; }
    public Dictionary<string, dynamic> Thumbnails { get; set; }

    /// <summary>
    /// Gets or sets whether the layer is being created, which lets its name be entered.
    /// </summary>
    [BindNever]
    public bool IsNew { get; set; }

    /// <summary>
    /// Gets or sets the editor of the layer, built by the display drivers of <see cref="LayerEditViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}
