using OrchardCore.Admin;
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
    /// The data context for rendering the layers with the configured layout.
    /// </summary>
    [BindNever]
    public AdminListContext List { get; set; }
}
