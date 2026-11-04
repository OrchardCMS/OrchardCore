using System.ComponentModel;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;

namespace OrchardCore.ContentFields.Fields;

[RequireFeatures("OrchardCore.ContentLocalization")]
public class LocalizationSetContentPickerField : ContentField
{
    [Description("The localization set IDs of the selected content items.")]
    public string[] LocalizationSets { get; set; } = [];
}
