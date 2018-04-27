using System.Collections.Generic;

namespace OrchardCore.Workflows.UserTasks.ViewModels
{
    public class UserTaskEventViewModel
    {
        public string Actions { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
