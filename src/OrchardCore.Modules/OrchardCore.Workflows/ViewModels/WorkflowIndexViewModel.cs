using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels
{
    public class WorkflowIndexViewModel
    {
        public WorkflowIndexViewModel()
        {
            Options = new WorkflowIndexOptions();
        }

        public WorkflowType WorkflowType { get; set; }
        public IList<WorkflowEntry> Workflows { get; set; }
        public WorkflowIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class WorkflowIndexOptions
    {
        public WorkflowIndexOptions()
        {
            Filter = WorkflowFilter.All;
        }

        public WorkflowBulkAction BulkAction { get; set; }
        public WorkflowFilter Filter { get; set; }

        public WorkflowOrder OrderBy { get; set; }

        [BindNever]
        public List<SelectListItem> WorkflowsSorts { get; set; }

        [BindNever]
        public List<SelectListItem> WorkflowsStatuses { get; set; }

        [BindNever]
        public List<SelectListItem> WorkflowsBulkAction { get; set; }
    }

    public class WorkflowEntry
    {
        public Workflow Workflow { get; set; }
        public long Id { get; set; }
        public bool IsChecked { get; set; }
    }

    public enum WorkflowFilter
    {
        All,
        Finished,
        Faulted
    }

    public enum WorkflowOrder
    {
        CreatedDesc,
        Created
    }

    public enum WorkflowBulkAction
    {
        None,
        Delete
    }
}
