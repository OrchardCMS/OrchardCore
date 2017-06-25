using System;
using System.Collections.Generic;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.ViewModels
{
    public class AdminEditViewModel {
        public string LocalId { get; set; }
        public bool IsLocal { get; set; }
        public IEnumerable<IActivity> AllActivities { get; set; }
        public WorkflowDefinitionViewModel WorkflowDefinition { get; set; }
        public Workflow Workflow { get; set; }
    }

    [Serializable]
    public class UpdatedActivityModel {
        public string ClientId { get; set; }
        public string Data { get; set; }
    }
}