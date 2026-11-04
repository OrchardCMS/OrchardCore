namespace OrchardCore.Layers.Models;

/// <summary>Configured zones available to layer widget editors.</summary>
public class LayerSettings
{
    /// <summary>Zone names in editor order. Changing this list does not move or delete widgets.</summary>
    public string[] Zones { get; set; } = [];
}
