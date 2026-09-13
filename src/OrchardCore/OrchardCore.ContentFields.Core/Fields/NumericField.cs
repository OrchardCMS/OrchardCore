using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields;

public class NumericField : ContentField
{
    [Description("The numeric value, or null when unset.")]
    public decimal? Value { get; set; }
}
