using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels
{
    public class WorkflowTypeViewModel
    {
        public WorkflowType WorkflowType { get; set; }
        public IList<dynamic> ActivityThumbnailShapes { get; set; }
        public IList<dynamic> ActivityDesignShapes { get; set; }
        public IList<LocalizedString> ActivityCategories { get; set; }
        public string WorkflowTypeJson { get; set; }
        public string State { get; set; }
        public string LocalId { get; set; }
        public bool LoadLocalState { get; set; }
        public int WorkflowCount { get; set; }
    }
}
