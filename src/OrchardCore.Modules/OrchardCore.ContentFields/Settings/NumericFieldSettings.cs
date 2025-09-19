using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.ContentFields.Settings;

public class NumericFieldSettings : FieldSettings
{
    public int Scale { get; set; }

    public decimal? Minimum { get; set; }

    public decimal? Maximum { get; set; }

    public string Placeholder { get; set; }

    public string DefaultValue { get; set; }
}
