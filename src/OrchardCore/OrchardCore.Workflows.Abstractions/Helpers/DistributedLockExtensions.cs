using System;
using System.Threading.Tasks;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Helpers
{
    public static class DistributedLockExtensions
    {
        /// <summary>
        /// Tries to acquire a lock on this workflow if it is atomic, otherwise returns true with a null locker.
        /// </summary>
        public static Task<(ILocker locker, bool locked)> TryAcquireWorkflowLockAsync(this IDistributedLock distributedLock, Workflow workflow)
        {
            if (workflow.IsAtomic())
            {
                return distributedLock.TryAcquireLockAsync(
                    "WFI_" + workflow.WorkflowId + "_LOCK",
                    TimeSpan.FromMilliseconds(workflow.LockTimeout),
                    TimeSpan.FromMilliseconds(workflow.LockExpiration));
            }

            return Task.FromResult<(ILocker, bool)>((null, true));
        }

        /// <summary>
        /// Tries to acquire a lock on this workflow type if it is atomic, otherwise returns true with a null locker.
        /// </summary>
        public static Task<(ILocker locker, bool locked)> TryAcquireWorkflowTypeLockAsync(this IDistributedLock distributedLock, WorkflowType workflowType)
        {
            if (workflowType.IsAtomic())
            {
                return distributedLock.TryAcquireLockAsync(
                    "WFT_" + workflowType.WorkflowTypeId + "_LOCK",
                    TimeSpan.FromMilliseconds(workflowType.LockTimeout),
                    TimeSpan.FromMilliseconds(workflowType.LockExpiration));
            }

            return Task.FromResult<(ILocker, bool)>((null, true));
        }
    }
}
