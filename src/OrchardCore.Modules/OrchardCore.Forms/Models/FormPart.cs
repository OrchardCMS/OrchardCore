using OrchardCore.ContentManagement;

namespace OrchardCore.Forms.Models
{
    public class FormPart : ContentPart
    {
        public string Action { get; set; }
        public string Method { get; set; }
        public string WorkflowTypeId { get; set; }
    }
}
