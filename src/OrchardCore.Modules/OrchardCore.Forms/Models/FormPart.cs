using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Forms.Models;

public class FormPart : ContentPart
{
    [Description("The URL to which the form is submitted.")]
    public string Action { get; set; }

    [Description("The HTTP method used to submit the form, typically GET or POST.")]
    public string Method { get; set; }

    [Description("The identifier of the workflow type started when the form is submitted.")]
    public string WorkflowTypeId { get; set; }

    [Description("The media type used to encode submitted form data.")]
    public string EncType { get; set; }

    [DefaultValue(true)]
    [Description("Whether the form includes an antiforgery token.")]
    public bool EnableAntiForgeryToken { get; set; } = true;

    [DefaultValue(true)]
    [Description("Whether the form stores its current URL for use after submission.")]
    public bool SaveFormLocation { get; set; } = true;
}
