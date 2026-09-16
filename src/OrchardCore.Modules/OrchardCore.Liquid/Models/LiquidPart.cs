using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Liquid.Models;

public class LiquidPart : ContentPart
{
    [Description("The Liquid template source for the content body.")]
    public string Liquid { get; set; }
}
