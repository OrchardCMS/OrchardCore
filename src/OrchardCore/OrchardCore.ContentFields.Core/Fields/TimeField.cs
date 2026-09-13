using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields;

public class TimeField : ContentField
{
    [Description("The time of day in hh:mm:ss format, or null when unset.")]
    public TimeSpan? Value { get; set; }
}
