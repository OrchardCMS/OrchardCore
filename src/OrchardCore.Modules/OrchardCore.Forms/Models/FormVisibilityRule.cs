namespace OrchardCore.Forms.Models;

public sealed class FormVisibilityRule
{
    public string Field { get; set; }

    public FormVisibilityOperator? Operator { get; set; }

    public string[] Values { get; set; }

    public bool CaseSensitive { get; set; }
}
