using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields;

public class BooleanField : ContentField
{
    [Description("The Boolean value.")]
    public bool Value { get; set; }
}
