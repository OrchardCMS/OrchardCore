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
        /// Tries to acquire a lock on this workflow type if it is a singleton or
        /// if the event is exclusive, otherwise returns true with a null locker.
        /// </summary>
        public static Task<(ILocker locker, bool locked)> TryAcquireWorkflowTypeLockAsync(
            this IDistributedLock distributedLock,
            WorkflowType workflowType,
            bool isExclusiveEvent = false)
        {
            if (workflowType.IsSingleton || isExclusiveEvent)
            {
                return distributedLock.TryAcquireLockAsync(
                    "WFT_" + workflowType.WorkflowTypeId + "_LOCK",
                    TimeSpan.FromMilliseconds(workflowType.LockTimeout.Value),
                    TimeSpan.FromMilliseconds(workflowType.LockExpiration.Value));
            }

            return Task.FromResult<(ILocker, bool)>((null, true));
        }

        /// <summary>
        /// Tries to acquire a lock on this workflow instance.
        /// </summary>
        public static Task<(ILocker locker, bool locked)> TryAcquireWorkflowLockAsync(
            this IDistributedLock distributedLock,
            Workflow workflow)
        {
            return distributedLock.TryAcquireLockAsync(
                "WFI_" + workflow.WorkflowId + "_LOCK",
                TimeSpan.FromMilliseconds(workflow.LockTimeout.Value),
                TimeSpan.FromMilliseconds(workflow.LockExpiration.Value));
        }
    }
}
