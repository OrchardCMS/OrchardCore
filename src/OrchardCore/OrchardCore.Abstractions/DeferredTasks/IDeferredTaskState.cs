using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.DeferredTasks
{
    /// <summary>
    /// An implementation of this interface is responsible for storing actions that need to be executed
    /// at then end of the active request.
    /// </summary>
    public interface IDeferredTaskState
    {
        IList<Func<DeferredTaskContext, Task>> Tasks { get; }
    }
}
