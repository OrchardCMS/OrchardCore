using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields;

public class DateTimeField : ContentField
{
    [Description("The date and time in ISO 8601 format, or null when unset.")]
    public DateTime? Value { get; set; }
}
