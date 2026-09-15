using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;
using OrchardCore.Layers.Models;

namespace OrchardCore.Layers.ViewModels;

public class LayersIndexViewModel
{
    [BindNever]
    public string[] Zones { get; set; }

    [BindNever]
    public Dictionary<string, List<dynamic>> Widgets { get; set; } = [];

    [BindNever]
    public List<Layer> Layers { get; set; }

    /// <summary>
    /// The <c>AdminList</c> shape rendering the layers with the configured layout.
    /// </summary>
    [BindNever]
    public IShape List { get; set; }
}
