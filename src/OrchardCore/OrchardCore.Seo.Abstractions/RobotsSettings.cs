using System.ComponentModel;

namespace OrchardCore.Seo;

public class RobotsSettings
{
    [DefaultValue(true)]
    public bool AllowAll { get; set; } = true;

    [DefaultValue(true)]
    public bool DiallowAdmin { get; set; } = true;

    public string AdditionalRules { get; set; }
}
