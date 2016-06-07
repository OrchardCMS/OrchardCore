using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.Processing
{
    public interface IDeferredTaskState
    {
        IList<Func<ProcessingEngineContext, Task>> Tasks { get; }
    }
}
