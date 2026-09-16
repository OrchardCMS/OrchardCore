using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Facebook.Widgets.Models;

public class FacebookPluginPart : ContentPart
{
    [Description("The Liquid template used to render the Facebook plugin.")]
    public string Liquid { get; set; }
}
