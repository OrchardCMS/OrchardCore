using System.Collections.Generic;

namespace OrchardCore.Users.Workflows.ViewModels
{
    public class ValidateUserTaskViewModel
    {
        public bool SetUser { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
}