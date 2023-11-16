using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Workflows.Http.Models;

namespace OrchardCore.Forms.ViewModels
{
    public class FormPartEditViewModel
    {
        public string Action { get; set; }

        public string Method { get; set; }

        public string WorkflowTypeId { get; set; }

        public string EncType { get; set; }

        public bool EnableAntiForgeryToken { get; set; } = true;

        public bool SaveFormLocation { get; set; } = true;

        public WorkflowPayload WorkflowPayload { get; set; } = new();

        [BindNever]
        public List<SelectListItem> WorkflowTypes { get; set; } = new();
    }
}
