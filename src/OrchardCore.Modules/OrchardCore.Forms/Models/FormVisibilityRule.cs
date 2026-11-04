using System.ComponentModel;

namespace OrchardCore.Forms.Models;

public sealed class FormVisibilityRule
{
    [Description("The form field whose value is evaluated.")]
    public string Field { get; set; }

    [Description("The comparison operator applied to the field value.")]
    public FormVisibilityOperator? Operator { get; set; }

    [Description("The values compared with the form field value.")]
    public string[] Values { get; set; }

    [Description("Whether string comparisons distinguish uppercase and lowercase characters.")]
    public bool CaseSensitive { get; set; }
}
