using System.Collections.Generic;

namespace OrchardCore.Users.Workflows.ViewModels
{
    public class ValidateUserTaskViewModel
    {
        public bool SetUser { get; set; }

        public IEnumerable<string> Roles { get; set; }
    }
}