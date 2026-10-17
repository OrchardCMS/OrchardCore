using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Helpers;

public static class DistributedLockExtensions
{
    /// <summary>
    /// Tries to acquire a lock before resuming this workflow instance, but only
    /// if it is an atomic workflow, otherwise returns true with a null locker.
    /// </summary>
    public static Task<(ILocker locker, bool locked)> TryAcquireWorkflowLockAsync(
        this IDistributedLock distributedLock,
        Workflow workflow)
    {
        if (workflow.IsAtomic)
        {
            return distributedLock.TryAcquireLockAsync(
                "WFI_" + workflow.WorkflowId + "_LOCK",
                TimeSpan.FromMilliseconds(workflow.LockTimeout),
                TimeSpan.FromMilliseconds(workflow.LockExpiration));
        }

        return Task.FromResult<(ILocker, bool)>((null, true));
    }

    /// <summary>
    /// Tries to acquire a lock before starting an instance of this workflow type, if it is
    /// a singleton or if the event is exclusive, or a lock per correlation id if it runs one instance
    /// per correlation id (<see cref="WorkflowType.IsSingletonPerCorrelation"/>), otherwise returns
    /// true with a null locker.
    /// </summary>
    public static Task<(ILocker locker, bool locked)> TryAcquireWorkflowTypeLockAsync(
        this IDistributedLock distributedLock,
        WorkflowType workflowType,
        bool isExclusiveEvent = false,
        string correlationId = null)
    {
        if (workflowType.IsSingleton || isExclusiveEvent)
        {
            return distributedLock.TryAcquireLockAsync(
                "WFT_" + workflowType.WorkflowTypeId + "_LOCK",
                TimeSpan.FromMilliseconds(20_000),
                TimeSpan.FromMilliseconds(20_000));
        }

        if (workflowType.IsSingletonPerCorrelation && !string.IsNullOrEmpty(correlationId))
        {
            return distributedLock.TryAcquireLockAsync(
                "WFT_" + workflowType.WorkflowTypeId + "_" + correlationId + "_LOCK",
                TimeSpan.FromMilliseconds(20_000),
                TimeSpan.FromMilliseconds(20_000));
        }

        return Task.FromResult<(ILocker, bool)>((null, true));
    }
}
