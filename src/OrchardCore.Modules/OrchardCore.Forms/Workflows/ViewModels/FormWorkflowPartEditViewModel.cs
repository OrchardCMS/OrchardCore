using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Forms.Workflows.ViewModels
{
    public class FormWorkflowPartEditViewModel
    {
        public string WorkflowTypeId { get; set; }

        public IList<SelectListItem> AvailableWorkflowTypes { get; set; }
    }
}
