using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Workflows.ViewModels
{
    public class WorkflowDefinitionPropertiesViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public bool IsEnabled { get; set; }
        public string ScriptingEngine { get; set; }
        public string ReturnUrl { get; set; }
        public IList<SelectListItem> AvailableScriptingEngines { get; set; }
    }
}