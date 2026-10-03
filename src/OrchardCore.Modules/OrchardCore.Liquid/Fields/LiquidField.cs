using OrchardCore.ContentManagement;

namespace OrchardCore.Liquid.Fields;

/// <summary>
/// Stores authored Liquid source for a content field.
/// </summary>
public class LiquidField : ContentField
{
    /// <summary>
    /// Gets or sets the authored Liquid source.
    /// </summary>
    public string Liquid { get; set; }
}
