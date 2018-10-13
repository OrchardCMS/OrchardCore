using System;
using System.Threading.Tasks;

namespace OrchardCore.DeferredTasks
{
    public class DeferredTask
    {
        public Func<DeferredTaskContext, Task> Task { get; set; }
        public int Order { get; set; }
    }
}
