using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields;

public class MultiTextField : ContentField
{
    [Description("The selected text values.")]
    public string[] Values { get; set; } = [];
}
