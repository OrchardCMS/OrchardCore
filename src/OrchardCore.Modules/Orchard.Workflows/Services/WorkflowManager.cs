using System;
using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Workflows.Services {
    public class WorkflowManager : IWorkflowManager
    {
        public void TriggerEvent(string name, IContent target, Func<Dictionary<string, object>> tokensContext)
        {
            
        }
    }
}
