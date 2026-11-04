using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Forms.Models;

public class ValidationSummaryPart : ContentPart
{
    [Description("Whether the summary shows only model-level errors instead of all validation errors.")]
    public bool ModelOnly { get; set; }
}
