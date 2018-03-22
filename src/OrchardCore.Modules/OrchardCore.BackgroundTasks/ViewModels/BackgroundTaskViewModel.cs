using System.Collections.Generic;
using OrchardCore.BackgroundTasks.Models;

namespace OrchardCore.BackgroundTasks.ViewModels
{
    public class BackgroundTaskViewModel : BackgroundTaskDefinition
    {
        public IEnumerable<string> Names { get; set; }
    }
}
