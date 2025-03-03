namespace OrchardCore.Forms.Models;

public class FormVisibilityRule
{
    public string Field { get; set; }

    public FormVisibilityOperator Operator { get; set; }

    public string[] Values { get; set; }

    public string TargetInputId { get; set; }
}
