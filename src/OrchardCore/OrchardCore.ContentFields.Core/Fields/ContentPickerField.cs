using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields;

public class ContentPickerField : ContentField
{
    [Description("The IDs of the selected content items.")]
    public string[] ContentItemIds { get; set; } = [];
}
