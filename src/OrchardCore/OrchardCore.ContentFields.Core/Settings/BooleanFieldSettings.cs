using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.ContentFields.Settings;

public class BooleanFieldSettings : FieldSettings
{
    public string Label { get; set; }

    public bool DefaultValue { get; set; }
}
