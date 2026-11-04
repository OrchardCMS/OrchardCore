using System.ComponentModel;

namespace OrchardCore.Forms.Models;

public sealed class FormVisibilityRuleGroup
{
    [Description("The visibility rules that must be satisfied by this group.")]
    public IEnumerable<FormVisibilityRule> Rules { get; set; }
}
