using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields;

public class DateField : ContentField
{
    [Description("The date in ISO 8601 format, or null when unset.")]
    public DateTime? Value { get; set; }
}
