using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.DeferredTasks
{
    public interface IDeferredTaskState
    {
        IList<Func<DeferredTaskContext, Task>> Tasks { get; }
    }
}
