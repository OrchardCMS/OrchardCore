using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Modules;

namespace OrchardCore.ContentFields.Settings;

[RequireFeatures("OrchardCore.ContentLocalization")]
public class LocalizationSetContentPickerFieldSettings : FieldSettings
{
    public bool Multiple { get; set; }

    public string[] DisplayedContentTypes { get; set; } = [];
}
